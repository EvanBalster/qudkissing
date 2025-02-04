using System;
using XRL.Core;
using XRL.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Qud.API;
using XRL.World.Parts.Effects;

namespace XRL.World.Parts
{
	[Serializable]
	public class acegiak_Kissable : IPart
	{
		//public string useFactionForFeelingFloor;

		public bool kissableIfPositiveFeeling;

		//private bool bOnlyAllowIfLiked = true;

		public List<acegiak_KissingPreference> preferences = null;



		public acegiak_Kissable()
		{
			base.Name = "acegiak_Kissable";
			//DisplayName = "Kissable";
			
		}


		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "GetInventoryActions");
			Object.RegisterPartEvent(this, "InvCommandKiss");
			base.Register(Object);
		}

		public void KissBuff(GameObject who){
			if(!who.HasEffect("acegiak_EffectGay")){
				who.ApplyEffect(new acegiak_EffectGay());
			}
		}

		public bool Kiss(GameObject who)
		{
            
                if (!hasPart(ParentObject,"face")){
					if (who.IsPlayer())
					{
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y doesn't have a face.");
					}
					return true;
                }
				if (!hasPart(who,"face")){
					if (who.IsPlayer())
					{
						Popup.Show("You don't have a face.");
					}
					return true;
                }

			string beguiled = "";
			string hbeguiled = " &Mkissed you back&y.";
			if(ParentObject.GetIntProperty("BeguilingBonusApplied") >0){
				beguiled = "\n"+ParentObject.It+ParentObject.GetVerb("quake")+" with fear and manic ecstasy.";
				hbeguiled = " could not resist.";
			}else{
                
				if (kissableIfPositiveFeeling && ParentObject.pBrain.GetFeeling(who) < 55)
				{
					if (who.IsPlayer())
					{
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y" + ParentObject.GetVerb("shy") + " away from you.");
					}
					ParentObject.pBrain.AdjustFeeling(who,-5);

					if(ParentObject.pBrain.GetFeeling(who) < 0){
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&Y is upset by your advances!");
					}
					return true;
				}
				if(!isAttractedTo(who)){
					if (who.IsPlayer())
					{
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&Y isn't attracted to you.");
					}
					ParentObject.pBrain.AdjustFeeling(who,-10);

					if(ParentObject.pBrain.GetFeeling(who) < 0){
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&Y is upset by your advances!");
					}
					return true;
				}
				
			}

			if(ParentObject.GetPart<acegiak_Romancable>() != null){

				if(ParentObject.GetPart<acegiak_Romancable>().patience<=0){
					ParentObject.pBrain.AdjustFeeling(who,-5);
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&Y doesn't want to kiss you right now.");

					if(ParentObject.pBrain.GetFeeling(who) < 0){
						Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&Y is upset by your advances!");
					}
				}
				ParentObject.GetPart<acegiak_Romancable>().patience--;
			}


            
			string verb = "kiss";
			GameObject parentObject = ParentObject;
			IPart.XDidYToZ(who, verb, parentObject, null, null, true);
			if (who.IsPlayer())
			{
				if (ParentObject.HasPropertyOrTag("SpecialKissResponse"))
				{
					Popup.Show(ParentObject.GetTag("SpecialKissResponse"));
				}
				else
				{
					KissBuff(who);
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y " + ParentObject.GetPropertyOrTag("KissResponse", "&Mkisses you back") + "."+beguiled);
					JournalAPI.AddAccomplishment("&y You kissed "+ParentObject.a + ParentObject.DisplayNameOnlyDirect +" and "+ParentObject.it+hbeguiled, "general", null, -1L);
				}
			}
			ParentObject.Heartspray();
			who.UseEnergy(1000, "Kissing");
			ParentObject.FireEvent(Event.New("ObjectKissed", "Object", ParentObject, "Kisser", who));
			return true;
		}

		public void havePreference(){
			if(preferences == null){
				preferences = new List<acegiak_KissingPreference>();
				Random random = new Random();
				int count = random.Next(5);
				for(int i = 0; i<count;i++){
					switch (random.Next(4)){
					case 0:
						preferences.Add(new acegiak_PartPreference(ParentObject));
						break;
					case 1:
						preferences.Add(new acegiak_AmorousPreference(ParentObject));
						break;
					case 2:
						preferences.Add(new acegiak_StatPreference(ParentObject));
						break;
					case 3:
						preferences.Add(new acegiak_RelationshipPreference(ParentObject));
						break;
					}
				}
			}
		}


        public bool isAttractedTo(GameObject kisser){
			havePreference();
			float sum = 0; 
			IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + "&c examines you.");
			List<string> many = new List<string>();
			foreach(acegiak_KissingPreference pref in preferences){
				acegiak_KissingPreferenceResult output = pref.attractionAmount(ParentObject,kisser);
				sum += output.amount;
				if(kisser.IsPlayer() && !many.Contains(output.explanation)){
					IPart.AddPlayerMessage("" + ParentObject.the + ParentObject.DisplayNameOnly + " "+output.explanation+".");
				}
				many.Add(output.explanation);

			}
            if(sum > 0){
                return true;
            }
            return false;
        }

        private bool hasPart(GameObject GO, string partname){
            Body part = GO.GetPart<Body>();
            if (part == null)
            {
				return false;
            }

			BodyPart bpart = part.GetBody();

			return bpart != null;
        }

		private bool partHasPart(XRL.World.BodyPart part, string partname){
			if(part.Type == partname){
				return true;
			}
			foreach(XRL.World.BodyPart subpart in part.Parts){
				if(subpart != part && partHasPart(subpart,partname)){
					return true;
				}
			}
			return false;
		}

		// public Conversation InjectRomance(Conversation conversation){

		// 	if(conversation == null
		// 		|| conversation.NodesByID == null
		// 		|| ! conversation.NodesByID.ContainsKey("Start")
		// 		|| conversation.NodesByID["Start"].Choices == null){
		// 			return conversation;
		// 	}
			
		// 	ConversationChoice returntostart = new ConversationChoice();
		// 	returntostart.Text = "Ok.";
		// 	returntostart.GotoID = "Start";

		// 	ConversationNode aboutme = new ConversationNode();
		// 	aboutme.ID = "acegiak_aboutme";
		// 	aboutme.Text = "I like long walks on the beach where sometimes I look back and there's only one set of footprints";
		// 	aboutme.Choices.Add(returntostart);

		// 	ConversationChoice romanticEnquiry = new ConversationChoice();
		// 	romanticEnquiry.Text = "Tell me a little about yourself.";
		// 	romanticEnquiry.GotoID = "acegiak_about";
			
		// 	conversation.NodesByID["Start"].Choices.Add(romanticEnquiry);
		// 	conversation.AddNode(aboutme);

		// 	return conversation;
		// }


		public override bool FireEvent(Event E){
            if (E.ID == "GetInventoryActions")
			{
				if(ParentObject.pBrain.GetFeeling(E.GetGameObjectParameter("Owner")) > 0){
					E.GetParameter<EventParameterGetInventoryActions>("Actions").AddAction("Kiss", 'k',  false, "&Wk&yiss", "InvCommandKiss", 10);
				}
			}
			if (E.ID == "InvCommandKiss" && Kiss(E.GetGameObjectParameter("Owner")))
			{
				if(ParentObject.pBrain.GetFeeling(E.GetGameObjectParameter("Owner")) > 0){
					E.RequestInterfaceExit();
				}
			}

			return base.FireEvent(E);
		}
	}
}
