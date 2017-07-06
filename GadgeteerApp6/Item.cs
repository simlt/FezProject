namespace GadgeteerApp
{
    internal class Item
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public bool Found { get; set; }

        internal Item() { Found = false; }
    }
}