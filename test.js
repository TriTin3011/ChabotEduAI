const url = 'https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key=AQ.Ab8RN6Kb9PnfHm6u7RVmCL9wNTpgx2h420LmsTN4m1dFlEh_dw';
const body = {
  model: "models/gemini-embedding-001",
  content: { parts: [{ text: "Hello" }] }
};
fetch(url, { method: 'POST', body: JSON.stringify(body), headers: { 'Content-Type': 'application/json' } })
  .then(res => res.text().then(t => console.log(res.status, t)));
