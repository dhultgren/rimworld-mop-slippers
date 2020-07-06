using RimWorld;
using System.Linq;
using Verse;
using Verse.Sound;

namespace MopSlippers
{
    public class ThingDef_MopSlippers : Apparel
    {
		public static float DegradationPerClean = 0.03f;

        private IntVec3 _lastTile = IntVec3.Invalid;
		private float _degradationToAdd = 0;

        public override void Tick()
        {
            base.Tick();
			if (Wearer == null) return;

			var pos = Wearer.Position;
            if (pos != null && _lastTile != pos && MapHeld != null && pos.IsValid)
            {
                _lastTile = pos;

				var filth = pos.GetThingList(MapHeld).FirstOrDefault(t => t.def.IsFilth) as Filth;
				if (filth != null)
				{
					CleanTile(filth);
				}
            }
        }

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _lastTile, "lastTile");
			Scribe_Values.Look(ref _degradationToAdd, "degradationToAdd");
		}

		private void CleanTile(Filth filth)
		{
			filth.ThinFilth();

			_degradationToAdd += DegradationPerClean;
			if (_degradationToAdd >= 1)
            {
				HitPoints -= 1;
				_degradationToAdd -= 1;
			}

			/*var sound = filth.def.filth.cleaningSound ?? SoundDefOf.Interact_CleanFilth;

			if (sound != null && !sound.sustain)
			{
				sound.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
			}*/
		}
    }
}
