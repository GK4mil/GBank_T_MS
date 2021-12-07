using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GBankTransferService.Model
{
    public class User
    {
        public User()
        {
            this.Bills = new Collection<Bill>();
        }
        public int ID { get; set; }
        public string Username { get; set; }
        public string password { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }

        public bool active { get; set; }

       

        [JsonIgnore]
        public virtual ICollection<Bill> Bills { get; set; }


    }
}

