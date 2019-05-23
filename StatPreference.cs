using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using Mono.CSharp;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_StatPreference : acegiak_RomancePreference
	{
        string Stat = "Ego";
        float Amount = 0.1f;
        int Needs = 0;

        public acegiak_StatPreference(GameObject GO){
            if(GO == null){
                return;
            }
            List<string> Stats = new List<string>(new string[] { "Strength", "Agility", "Toughness", "Ego", "Wisdom","Intelligence" });

            Random random = new Random();
            this.Stat = Stats[random.Next(Stats.Count-1)];
            this.Amount =  (float)((random.NextDouble()*2)-1);
            this.Needs =  random.Next(-3,5);
        }

        public Tuple<float,string> attractionAmount(GameObject GO){
            //             IPart.AddPlayerMessage("They "+(Amount>0?"like ":"dislike ")+this.Stat+" over "+this.Needs.ToString());

            // IPart.AddPlayerMessage("Your "+Stat+(GO.StatMod(Stat)>=Needs?" meets ":" does not meet ")+this.Needs.ToString());


            float result = Amount * (GO.StatMod(Stat)>=Needs?1:-1);
            string explain = ((result>0)?"is attracted to":"is &rnot attracted to")+" your "+((GO.StatMod(Stat)>=Needs)?"high ":"low ")+Stat;
            return new Tuple<float,string>(result,explain);
        }

    }
}