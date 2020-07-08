using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MopSlippers
{
    public class ThingDef_MopSlippers : Apparel
    {
		private static readonly float DegradationPerClean = 0.03f;
		private static readonly int MaxFilth = 10;

        private IntVec3 _lastTile = IntVec3.Invalid;
		private float _degradationToAdd = 0;
        private string _lastPawnFilth;

        private List<Filth> CarriedFilth { get; } = new List<Filth>();

        public override void Tick()
        {
            base.Tick();
			if (Wearer == null) return;

			var pos = Wearer.Position;
            if (pos != null && _lastTile != pos && MapHeld != null && pos.IsValid)
            {
				_lastTile = pos;

				var filth = pos.GetThingList(MapHeld).FirstOrDefault(t => t.def.IsFilth) as Filth;
				var pawnChangedFilth = PawnChangedCarriedFilthOnThisTile();
				if (!pawnChangedFilth && filth != null)
				{
					CleanTile(filth);
				}
				if (CarriedFilth.Any())
				{
					TryDropFilth();
				}
            }
        }

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _degradationToAdd, "degradationToAdd");
		}

		private void CleanTile(Filth filth)
		{
			filth.ThinFilth();
			if (filth.thickness > 0 && TryAddCarriedFilth(filth))
			{
				filth.Destroy();
			}

			_degradationToAdd += DegradationPerClean;
			if (_degradationToAdd >= 1)
			{
				HitPoints -= 1;
				_degradationToAdd -= 1;
			}
		}

		private void TryDropFilth()
		{
			var f = CarriedFilth.Last();
			if (f.CanDropAt(Wearer.Position, Wearer.Map) && FilthMaker.TryMakeFilth(Wearer.Position, Wearer.Map, f.def, f.sources))
			{
				f.ThinFilth();
				if (f.thickness <= 0)
				{
					CarriedFilth.Remove(f);
				}
			}
		}

		private bool TryAddCarriedFilth(Filth filth)
        {
			var totalFilth = CarriedFilth.Sum(f => f.thickness);

			if (totalFilth + filth.thickness <= MaxFilth)
            {
				CarriedFilth.Add(filth);
				return true;
            }
			return false;
		}

		// Unfortunately I can't directly check pawn filth, so just don't do anything if it has changed this tile.
		// This is to avoid cleaning filth that was just deposited.
		private bool PawnChangedCarriedFilthOnThisTile()
		{
			if (Wearer?.filth?.FilthReport != _lastPawnFilth)
			{
				_lastPawnFilth = Wearer?.filth?.FilthReport;
				return true;
			}
			return false;
		}
	}
}
