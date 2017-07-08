using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebService.Models
{
    public class Game
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameID { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }

    [DataContract(Name = "Game")]
    public class GameDTO
    {
        [DataMember]
        public int GameID { get; set; }
        [DataMember]
        public ICollection<ItemDTO> Items { get; set; }

        internal GameDTO(Game game)
        {
            GameID = game.GameID;
            Items = game.Items.Select(i => new ItemDTO(i)).ToList();
        }
    }
}