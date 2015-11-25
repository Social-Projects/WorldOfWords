using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using WorldOfWords.Domain.Services.IServices;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace WorldofWords.Hubs
{
    [HubName("ticketNotification")]
    [TicketHubAuthorization]
    public class TicketNotificationHub : Hub
    {
        public override System.Threading.Tasks.Task OnConnected()
        {
            var roles = Context.QueryString.Get("role");
            if (roles.Contains("Admin"))
            {
                Groups.Add(Context.ConnectionId, "Admins");
                if(roles.Contains("Student") || roles.Contains("Teacher"))
                {
                    Clients.Caller.updateUnreadTicketCounterForUser();
                }
                return Clients.Caller.updateUnreadTicketCounterForAdmin();
            }
            return Clients.Caller.updateUnreadTicketCounterForUser();
        }

        public void NotifyAboutChangeTicketState(string ownerId, string subject, string reviewStatus)
        {
            UpdateTicketTable(ownerId);
            UpdateUnreadTicketCounterForUser(ownerId);
            UpdateUnreadTicketCounterForAdmin();
            Clients.User(ownerId).notifyAboutChangeTicketState(subject, reviewStatus);
        }

        public void UpdateTicketTable(string ownerId)
        {
            Clients.User(ownerId).updateTicketTable();
            Clients.Group("Admins").updateTicketTable();
        }

        public void NotifyAdminsAboutNewTicket(string subject, string ownerId)
        {
            UpdateTicketTable(ownerId);
            UpdateUnreadTicketCounterForAdmin();
            Clients.Group("Admins").notifyAboutNewTicket(subject);
        }

        public void NotifyAboutSharedWordSuites(string[] teachersToShareId)
        {
            foreach (var id in teachersToShareId)
            {
                Clients.User(id).notifyAboutSharedWordSuites();
            }
        }

        public void UpdateUnreadTicketCounterForAdmin()
        {
             Clients.Group("Admins").updateUnreadTicketCounterForAdmin();
        }

        public void UpdateUnreadTicketCounterForUser(string ownerId)
        {
             Clients.User(ownerId).updateUnreadTicketCounterForUser();
        }
    }
}