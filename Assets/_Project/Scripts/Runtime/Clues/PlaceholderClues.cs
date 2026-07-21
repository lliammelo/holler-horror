using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// Canned Wendigo-case clues for the M2 slice, before evidence pickups (M4)
    /// and NPC testimony (M3) generate real cards.
    /// </summary>
    public static class PlaceholderClues
    {
        private static readonly (string title, string body, ClueKind kind, string source)[] Pool =
        {
            ("Stripped carcass", "A hog, taken apart with nothing wasted. No animal eats like this.", ClueKind.Evidence, "Farmstead smokehouse"),
            ("Claw marks, head height", "Bark scored seven feet up the ash tree line.", ClueKind.Evidence, "Ridge trail"),
            ("Scream after midnight", "\"Heard it come down the valley. Weren't no panther.\"", ClueKind.Testimony, "Ada Bricker"),
            ("The winter the food ran out", "\"Ask what happened to Elias Cole in '31. Ask who came back fat.\"", ClueKind.Testimony, "Old Man Tetch"),
            ("Fogged mirror", "The hall mirror won't wipe clear. Water sits still and grey.", ClueKind.Evidence, "Widow Combs' house"),
            ("Duplicate footprints", "Two sets, same boots, ten minutes apart. One walks into the creek.", ClueKind.Evidence, "Creek crossing"),
            ("A conversation denied", "\"I never told Ruth about the cellar. Whoever she talked to, it weren't me.\"", ClueKind.Testimony, "Harlan Boggs"),
            ("Dead birdsong", "The insects stop at the tree line. The quiet has an edge to it.", ClueKind.Evidence, "North thicket"),
            ("Candle bent to no wind", "Chapel candles lean west, all of them, flames steady.", ClueKind.Evidence, "The Chapel"),
            ("Paths that moved", "\"That trail's run past the mine mouth sixty years. Last night it didn't.\"", ClueKind.Testimony, "Granny Slone"),
            ("Smokehouse raided", "Meat gone, door torn from the top hinge downward.", ClueKind.Evidence, "Boggs homestead"),
            ("Livestock sick with fear", "The mules won't leave the barn. Won't eat either.", ClueKind.Testimony, "Ruth Combs"),
        };

        public static void PinRandom(Vector2 boardPosition)
        {
            if (ClueBoard.Instance == null)
                return;

            var (title, body, kind, source) = Pool[Random.Range(0, Pool.Length)];
            ClueBoard.Instance.AddCard(title, body, kind, source, boardPosition);
        }
    }
}
