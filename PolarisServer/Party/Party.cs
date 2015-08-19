using PolarisServer.Models;
using PolarisServer.Packets.PSOPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Party
{
    public class Party
    {
        public string name;
        private List<Client> members;
        private Client host;
        public Quest currentQuest;

        public Party(string name, Client host)
        {
            this.name = name;
            this.host = host;
            this.members = new List<Client>();
            addClientToParty(host);
        }

        public void addClientToParty(Client c)
        {
            if (members.Count < 1)
            {
                c.SendPacket(new PartyInitPacket(new Models.Character[1] { c.Character }));
            }
            else
            {
                // ???
            }
            members.Add(c);
            c.currentParty = this;
        }

        public void removeClientFromParty(Client c)
        {
            if(!members.Contains(c))
            {
                Logger.WriteWarning("[PTY] Client {0} was trying to be removed from {1}, but he was never in {1}!", c.User.Username, name);
                return;
            }

            members.Remove(c);
            //TODO do stuff like send the "remove from party" packet.
        }

        public bool hasClientInParty(Client c)
        {
            return members.Contains(c);
        }

        public Client getPartyHost()
        {
            return host;
        }

        public int getSize()
        {
            return members.Count;
        }

        public List<Client> getMembers()
        {
            return members;
        }

    }
}
