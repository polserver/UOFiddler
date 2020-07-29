namespace Ultima
{
    public sealed class LocationPointer
    {
        public int PointerX { get; }
        public int PointerY { get; }
        public int PointerZ { get; }
        public int PointerF { get; }

        public int SizeX { get; }
        public int SizeY { get; }
        public int SizeZ { get; }
        public int SizeF { get; }

        public LocationPointer(int ptrX, int ptrY, int ptrZ, int ptrF, int sizeX, int sizeY, int sizeZ, int sizeF)
        {
            PointerX = ptrX;
            PointerY = ptrY;
            PointerZ = ptrZ;
            PointerF = ptrF;

            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
            SizeF = sizeF;
        }
    }
}