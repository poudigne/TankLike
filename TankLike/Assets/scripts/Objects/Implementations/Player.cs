using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Objects.Interfaces;

namespace Objects.Implementations
{
	public class Player : IPlayer 
	{
		public string playerName { get; set; }
		public int tankID { get; set; }


		public Player(string name, int id)
		{
			playerName = name;
			tankID = id;
		}


	}
}

