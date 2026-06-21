using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Pgvector;

namespace BussinessLayer.Services
{
    public class ChatService : IChatService
    {
        private const string DefaultSessionTitle = "Cuộc trò chuyện mới";

        private readonly IChatRepository _chatRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IGeminiService _geminiService;
        private readonly IUserRepository _userRepository;

        public static int GetMonthlyLimit(string plan) => plan switch
        {
            "Basic" => 100,
            "Premium" => int.MaxValue,
            _ => 5
        };

        public ChatService(
            IChatRepository chatRepository,
            IDocumentRepository documentRepository,
            IGeminiService geminiService,
            IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _documentRepository = documentRepository;
            _geminiService = geminiService;
            _userRepository = userRepository;
        }

        public async Task<List<ChatSessionDto>> GetUserSessionsAsync(int userId)
        {
            var sessions = await _chatRepository.GetUserSessionsAsync(userId);
            return sessions.Select(MapSession).ToList();
        }

        public async Task<ChatSessionDto?> CreateSessionAsync(int userId)
        {
            var session = await _chatRepository.CreateSessionAsync(userId, DefaultSessionTitle);
            return MapSession(session);
        }

        public async Task<bool> DeleteSessionAsync(int userId, int sessionId)
        {
            var session = await _chatRepository.GetSessionByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return false;
            }

            await _chatRepository.DeleteSessionAsync(sessionId);
            return true;
        }

        public async Task<bool> ClearSessionAsync(int userId, int sessionId)
        {
            var session = await _chatRepository.GetSessionByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return false;
            }

            await _chatRepository.ClearSessionAsync(sessionId);
            return true;
        }

        public async Task<ChatResponseDto> ProcessChatMessageAsync(int userId, ChatRequestDto request)
        {
            try
            {
                var existingSession = await ResolveSessionAsync(userId, request.SessionId, request.Message, createIfMissing: false);
                var conversationHistory = BuildConversationHistory(existingSession?.Messages);

                var user = await _userRepository.GetUserByIdAsync(userId);
                var effectivePlan = "Free";
                var limit = GetMonthlyLimit("Free");
                var remainingBefore = int.MaxValue;
                var remainingAfter = int.MaxValue;

                if (user != null)
                {
                    var now = DateTime.UtcNow;
                    if (user.QuotaResetDate == null || now >= user.QuotaResetDate)
                    {
                        user.MonthlyQuestionCount = 0;
                        user.QuotaResetDate = DateTime.SpecifyKind(
                            new DateTime(now.Year, now.Month, 1).AddMonths(1),
                            DateTimeKind.Utc);
                        await _userRepository.UpdateUserAsync(user);
                    }

                    var planActive = user.SubscriptionPlan == "Free" ||
                                     (user.SubscriptionExpiry.HasValue && user.SubscriptionExpiry.Value >= now);
                    effectivePlan = planActive ? user.SubscriptionPlan : "Free";

                    limit = GetMonthlyLimit(effectivePlan);
                    remainingBefore = limit == int.MaxValue ? int.MaxValue : Math.Max(0, limit - user.MonthlyQuestionCount);
                    remainingAfter = remainingBefore == int.MaxValue ? int.MaxValue : Math.Max(0, remainingBefore - 1);

                    if (user.MonthlyQuestionCount >= limit)
                    {
                        return new ChatResponseDto
                        {
                            Success = false,
                            OutOfQuota = true,
                            Remaining = remainingBefore,
                            Message = effectivePlan == "Free"
                                ? $"Ban da dung het {limit} cau hoi mien phi trong thang nay. Nang cap goi de tiep tuc!"
                                : $"Ban da dat gioi han {limit} cau hoi/thang cua goi {effectivePlan}."
                        };
                    }
                }

                var citations = new List<CitationDto>();
                var contextText = string.Empty;

                if (request.SelectedDocIds != null && request.SelectedDocIds.Any())
                {
                    var questionEmbedding = await _geminiService.GetEmbeddingAsync(request.Message);
                    var similarChunks = await _documentRepository.SearchSimilarChunksAsync(
                        new Vector(questionEmbedding),
                        request.SelectedDocIds,
                        topK: 20); // Top-K Retrieval

                    if (similarChunks.Any())
                    {
                        // Thuc hien Re-ranking de chon ra 5 chunk tot nhat bang LLM
                        similarChunks = await RerankChunksAsync(request.Message, similarChunks, topN: 5);

                        contextText = string.Join(
                            "\n\n",
                            similarChunks.Select((chunk, index) =>
                                $"Nguon {index + 1}:\n" +
                                $"Tai lieu: {chunk.Document.Title}\n" +
                                $"Mon: {chunk.Document.Subject?.Name ?? "Khong ro"}\n" +
                                $"Chuong: {chunk.Document.Chapter?.Title ?? "Khong ro"}\n" +
                                $"Doan: {chunk.OrderIndex}\n" +
                                $"Noi dung: {chunk.Content}"));

                        citations = similarChunks
                            .Select(chunk => new CitationDto
                            {
                                DocumentId = chunk.DocumentId,
                                DocumentTitle = chunk.Document.Title,
                                SubjectName = chunk.Document.Subject?.Name,
                                ChapterTitle = chunk.Document.Chapter?.Title,
                                ChunkOrderIndex = chunk.OrderIndex,
                                Snippet = BuildSnippet(chunk.Content)
                            })
                            .ToList();
                    }
                    else
                    {
                        var docs = await _documentRepository.GetDocumentsByIdsAsync(request.SelectedDocIds);
                        foreach (var doc in docs)
                        {
                            var snippet = BuildSnippet(doc.Content);
                            contextText += $"Tai lieu: {doc.Title}\nNoi dung: {snippet}\n\n";
                            citations.Add(new CitationDto
                            {
                                DocumentId = doc.Id,
                                DocumentTitle = doc.Title,
                                ChunkOrderIndex = 0,
                                Snippet = snippet
                            });
                        }
                    }
                }

                if (request.RestrictToDocs)
                {
                    if (request.SelectedDocIds == null || !request.SelectedDocIds.Any())
                    {
                        return new ChatResponseDto
                        {
                            Success = false,
                            Message = "Hãy chọn ít nhất một tài liệu trước khi hỏi trong chế độ giơi hạn theo tài liệu."
                        };
                    }

                    if (string.IsNullOrWhiteSpace(contextText))
                    {
                        return new ChatResponseDto
                        {
                            Success = false,
                            Message = "Toi khong tim thay doan tai lieu phu hop de tra loi cau hoi nay trong cac tai lieu da chon."
                        };
                    }
                }

                var prompt = BuildPrompt(request.Message, conversationHistory, contextText, request.RestrictToDocs, effectivePlan, remainingAfter);
                var replyText = await _geminiService.GenerateAnswerAsync(prompt, request.ModelName);

                if (string.IsNullOrWhiteSpace(replyText))
                {
                    return new ChatResponseDto
                    {
                        Success = false,
                        Message = "AI khong tra ve noi dung hop le."
                    };
                }

                var session = existingSession ?? await ResolveSessionAsync(userId, request.SessionId, request.Message, createIfMissing: true);
                if (session == null)
                {
                    return new ChatResponseDto
                    {
                        Success = false,
                        Message = "Khong the tao hoac tim thay phien chat."
                    };
                }

                var title = session.Title;
                if (string.IsNullOrWhiteSpace(title) || title == DefaultSessionTitle)
                {
                    title = BuildSessionTitle(request.Message);
                }

                await _chatRepository.AddMessageAsync(new ChatMessage
                {
                    ChatSessionId = session.Id,
                    Role = "user",
                    Text = request.Message,
                    Timestamp = DateTime.UtcNow
                });

                await _chatRepository.AddMessageAsync(new ChatMessage
                {
                    ChatSessionId = session.Id,
                    Role = "model",
                    Text = replyText,
                    CitationPayloadJson = SerializeCitations(citations),
                    Timestamp = DateTime.UtcNow
                });

                if (title != session.Title)
                {
                    await _chatRepository.UpdateSessionTitleAsync(session.Id, title);
                }

                if (user != null)
                {
                    user.MonthlyQuestionCount++;
                    await _userRepository.UpdateUserAsync(user);
                }

                return new ChatResponseDto
                {
                    Success = true,
                    Reply = replyText,
                    Remaining = remainingAfter,
                    SessionId = session.Id,
                    SessionTitle = title,
                    Citations = citations
                };
            }
            catch (Exception ex)
            {
                return new ChatResponseDto
                {
                    Success = false,
                    Message = "Loi he thong: " + ex.Message
                };
            }
        }

        private async Task<ChatSession?> ResolveSessionAsync(int userId, int? sessionId, string message, bool createIfMissing)
        {
            if (sessionId.HasValue && sessionId.Value > 0)
            {
                var existing = await _chatRepository.GetSessionByIdAsync(sessionId.Value);
                if (existing != null && existing.UserId == userId)
                {
                    return existing;
                }
            }

            if (!createIfMissing)
            {
                return null;
            }

            var title = BuildSessionTitle(message);
            return await _chatRepository.CreateSessionAsync(userId, title);
        }

        private static string BuildSessionTitle(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return DefaultSessionTitle;
            }

            return message.Length > 22 ? message[..22] + "..." : message;
        }

        private static ChatSessionDto MapSession(ChatSession session)
        {
            return new ChatSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                Messages = session.Messages
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new ChatMessageDto
                    {
                        Role = m.Role,
                        Text = m.Text,
                        Timestamp = m.Timestamp,
                        Citations = DeserializeCitations(m.CitationPayloadJson)
                    })
                    .ToList()
            };
        }

        private static string BuildSnippet(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var normalized = content.Replace("\r", " ").Replace("\n", " ").Trim();
            return normalized.Length > 220 ? normalized[..220] + "..." : normalized;
        }

        private static string? SerializeCitations(List<CitationDto> citations)
        {
            if (citations == null || citations.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(citations);
        }

        private static List<CitationDto> DeserializeCitations(string? citationPayloadJson)
        {
            if (string.IsNullOrWhiteSpace(citationPayloadJson))
            {
                return new List<CitationDto>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CitationDto>>(citationPayloadJson) ?? new List<CitationDto>();
            }
            catch
            {
                return new List<CitationDto>();
            }
        }

        private static string BuildConversationHistory(IEnumerable<ChatMessage>? messages, int maxMessages = 8)
        {
            if (messages == null)
            {
                return string.Empty;
            }

            var recentMessages = messages
                .OrderBy(m => m.Timestamp)
                .TakeLast(maxMessages)
                .Select(m =>
                {
                    var roleLabel = m.Role == "user" ? "Nguoi dung" : "Tro ly AI";
                    return $"{roleLabel}: {m.Text}";
                })
                .ToList();

            return recentMessages.Count == 0
                ? string.Empty
                : string.Join("\n", recentMessages);
        }

        private static string BuildPrompt(
            string message,
            string conversationHistory,
            string contextText,
            bool restrictToDocs,
            string planName,
            int remainingQueries)
        {
            var promptSections = new List<string>();

            // Them thong tin he thong ve goi cuoc va so luot hoi con lai
            if (remainingQueries != int.MaxValue)
            {
                promptSections.Add(
                    $"[THONG TIN HE THONG]\nNguoi dung dang su dung goi: {planName}.\nSo luot hoi con lai trong thang sau cau hoi nay: {remainingQueries} luot.\n(Neu nguoi dung hoi ve so luot con lai, hoac lien quan den gioi han, hay dung thong tin nay de tra loi hoac nhac nho. Khong can nhac den neu khong lien quan).");
            }
            else
            {
                promptSections.Add(
                    $"[THONG TIN HE THONG]\nNguoi dung dang su dung goi: {planName} (Khong gioi han so luot hoi).");
            }

            if (!string.IsNullOrWhiteSpace(conversationHistory))
            {
                promptSections.Add(
                    "Lich su hoi thoai gan day:\n" +
                    conversationHistory +
                    "\n\nHay giu dung ngu canh hoi thoai khi tra loi cau hoi moi.");
            }

            if (!string.IsNullOrWhiteSpace(contextText))
            {
                if (restrictToDocs)
                {
                    promptSections.Add(
                        "Tai lieu lien quan:\n" +
                        contextText +
                        "\n\nChi su dung thong tin trong tai lieu tren de tra loi. Neu tai lieu khong du thong tin, hay noi ro rang.");
                }
                else
                {
                    promptSections.Add(
                        "Tai lieu lien quan (co the tham khao):\n" +
                        contextText +
                        "\n\nHay uu tien su dung thong tin trong tai lieu nay. Neu tai lieu khong du thong tin, ban co the su dung kien thuc san co cua ban de tra loi.");
                }
            }

            promptSections.Add($"Cau hoi hien tai: {message}");
            return string.Join("\n\n", promptSections);
        }

        private async Task<List<DocumentChunk>> RerankChunksAsync(string query, List<DocumentChunk> chunks, int topN)
        {
            if (chunks.Count <= topN)
            {
                return chunks;
            }

            var promptSections = new List<string>
            {
                "Ban la mot he thong cham diem muc do lien quan cua tai lieu. Nhiem vu cua ban la chon ra cac doan tai lieu phu hop nhat voi cau hoi.",
                $"Cau hoi: {query}",
                "Danh sach cac doan tai lieu:"
            };

            for (int i = 0; i < chunks.Count; i++)
            {
                promptSections.Add($"[{i}] {chunks[i].Content}");
            }

            promptSections.Add($@"Vui long tra ve MANG JSON gom toi da {topN} chi so (index) cua cac doan tai lieu lien quan nhat den cau hoi, sap xep theo do muc do phu hop giam dan. 
Vi du: [3, 0, 1, 5, 2]
CHI TRA VE MANG JSON, KHONG GIAI THICH HOAC THEM BAT KY VAN BAN NAO KHAC.");

            var prompt = string.Join("\n\n", promptSections);
            
            try
            {
                // Su dung gemini-1.5-flash cho nhiem vu re-rank de dam bao toc do
                var reply = await _geminiService.GenerateAnswerAsync(prompt, "gemini-1.5-flash");
                
                // Thu trich xuat JSON array tu phan hoi
                var jsonStart = reply.IndexOf('[');
                var jsonEnd = reply.LastIndexOf(']');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonStr = reply.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var indices = JsonSerializer.Deserialize<List<int>>(jsonStr);
                    if (indices != null && indices.Count > 0)
                    {
                        var reranked = new List<DocumentChunk>();
                        var addedIndices = new HashSet<int>();
                        foreach (var idx in indices)
                        {
                            if (idx >= 0 && idx < chunks.Count && !addedIndices.Contains(idx))
                            {
                                reranked.Add(chunks[idx]);
                                addedIndices.Add(idx);
                            }
                        }
                        
                        // Neu thieu so luong thi bo sung tu ban dau
                        if (reranked.Count < topN)
                        {
                            var remaining = chunks.Where((c, i) => !addedIndices.Contains(i)).Take(topN - reranked.Count);
                            reranked.AddRange(remaining);
                        }
                        return reranked;
                    }
                }
            }
            catch
            {
                // Fallback: neu loi thi tra ve topN ban dau
            }

            return chunks.Take(topN).ToList();
        }
    }
}
