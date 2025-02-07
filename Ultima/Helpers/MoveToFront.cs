using System;

namespace Ultima.Helpers
{
    public static class MoveToFrontCoding
    {
        // complexity : O(256*N) -> O(N)
        public static byte[] Encode(byte[] input)
        {
            Span<byte> symbols = stackalloc byte[256];
            byte[] output = new byte[input.Length];

            for (int i = 0; i < 256; i++)
            {
                symbols[i] = (byte)i;
            }

            for (int i = 0; i < input.Length; i++)
            {
                int ind = MoveToFront(symbols, input[i]);
                output[i] = (byte)ind;
            }

            return output;
        }

        // complexity : O(256*N) -> O(N)
        public static byte[] Decode(byte[] input)
        {
            Span<byte> symbols = stackalloc byte[256];
            byte[] output = new byte[input.Length];

            for (int i = 0; i < 256; i++)
            {
                symbols[i] = (byte)i;
            }

            for (int i = 0; i < input.Length; i++)
            {
                int ind = input[i];
                output[i] = symbols[ind];

                MoveToFront(symbols, ind);
            }

            return output;
        }

        // params : array, element to move
        // get the index of the element and move it to the front
        // best case : O(1), average and worst Case : O(N)
        private static int MoveToFront(Span<byte> array, byte element)
        {
            if (array[0] == element)
            {
                return 0;
            }

            int elementInd = -1;

            for (int i = array.Length - 1; i > 0; i--)
            {
                if (array[i] == element)
                {
                    elementInd = i;
                }

                if (elementInd != -1)
                {
                    array[i] = array[i - 1];
                }
            }

            array[0] = element;

            return elementInd;
        }

        // params : array, index of the element
        // move element to the front
        // complexity : O(elementInd)
        // best case : O(1), worst case : O(N)
        private static void MoveToFront(Span<byte> array, int elementInd)
        {
            byte element = array[elementInd];

            for (int i = elementInd; i > 0; i--)
            {
                array[i] = array[i - 1];
            }

            array[0] = element;
        }
    }
}
