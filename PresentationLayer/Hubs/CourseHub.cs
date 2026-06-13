using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PresentationLayer.Hubs
{
    public class CourseHub : Hub
    {
        // Clients will connect to this hub. 
        // We can define methods here if clients need to send messages to the server,
        // but for now, we just need the server to push notifications to clients.
    }
}
