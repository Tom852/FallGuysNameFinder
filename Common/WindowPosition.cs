namespace Common
{
    public struct WindowPosition
    {
        public WindowPosition(int left, int right, int top, int bottom) => (Left, Right, Top, Bottom) = (left, right, top, bottom);

        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }
}