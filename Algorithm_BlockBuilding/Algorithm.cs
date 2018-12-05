using System.Collections.Generic;
using MethodTimer;

namespace Algorithm_BlockBuilder
{
    internal class Algorithm
    {
        private readonly int MAX_OFFSET_BETWEEN_SERVICES;

        public Algorithm(int MAX_OFFSET_BETWEEN_SERVICES)
        {
            this.MAX_OFFSET_BETWEEN_SERVICES = MAX_OFFSET_BETWEEN_SERVICES;

        }

        [Time]
        public List<Block> Run(ServiceVariantSlot[] slots)
        {
            HashSet<char> allServices = new HashSet<char>();
            for (int i = 0; i < slots.Length; ++i)
                allServices.Add(slots[i].Code);

            var matrix = GenerateMatrix(slots);
            var result = Start(matrix, allServices, slots);
            return result;
        }

        /// <summary>
        /// Założenie: slots - posortowane po czasie rozpoczęcia
        /// </summary>
        /// <param name="slots"></param>
        /// <returns></returns>
        private int?[][] GenerateMatrix(ServiceVariantSlot[] slots)
        {
            //slots - posortowane po czasie rozpoczęcia
            int?[][] matrix = new int?[slots.Length - 1][];
            for (int i = 0; i < matrix.Length; ++i)
            {
                matrix[i] = new int?[matrix.Length - i];
                for (int j = 0; j < matrix.Length - i; ++j)
                {
                    var SV1 = slots[i];
                    var SV2 = slots[i + j + 1];
                    var timeOffset = SV2.StartTime - SV1.EndTime;
                    if (SV1.Code != SV2.Code && timeOffset >= 0)
                        matrix[i][j] = timeOffset;
                }
            }

            return matrix;
        }

        List<Block> Start(int?[][] matrix, HashSet<char> allServices, ServiceVariantSlot[] slots)
        {
            List<Block> listOfBlocks = new List<Block>();

            //TODO: call it parallel ?
            for (int b = 0; b < matrix.Length; ++b)
            {
                var remainingServices = new HashSet<char>(allServices);
                var firstService = slots[b];
                remainingServices.Remove(firstService.Code);
                var remainingPart = Search(matrix, b, remainingServices, slots, firstService);
                if (remainingServices.Count == 0)
                {
                    var result = new List<ServiceVariantSlot>() { firstService };
                    result.AddRange(remainingPart);
                    var block = new Block();
                    block.ServiceVariantSlots = result.ToArray();
                    listOfBlocks.Add(block);
                    if (block.SpareTime == 0)
                        break;
                }
            }
            
            return listOfBlocks.Count > 0 ? listOfBlocks : null;
        }

        private List<ServiceVariantSlot> Search(int?[][] matrix, int startIndex, HashSet<char> remainingServices, ServiceVariantSlot[] slots, ServiceVariantSlot previousService)
        {
            for (int j = 0; j < matrix[startIndex].Length && remainingServices.Count > 0; ++j)
            {
                if (matrix[startIndex][j] == null)
                    continue;
                var nextServiceIndex = startIndex + j + 1;
                var nextService = slots[nextServiceIndex];
                if (!remainingServices.Contains(nextService.Code)) //ta usługa jest już dobrana
                    continue;
                if (nextService.StartTime < previousService.EndTime) //nie bierz, jeżeli slot zachodzi na ostatni wybrany slot
                    continue;                                           //TODO: max interval - here or while generating matrix ?
                if (previousService.EndTime + MAX_OFFSET_BETWEEN_SERVICES < nextService.StartTime)
                    continue;

                remainingServices.Remove(nextService.Code);
                if (remainingServices.Count == 0)
                {
                    return new List<ServiceVariantSlot>() { nextService };
                }
                ///
                if (nextServiceIndex >= matrix.Length) //są jeszcze usługi do umówienia a nie ma już slotów
                {
                    remainingServices.Add(nextService.Code);
                    return null;
                }
                var remainingPart = Search(matrix, nextServiceIndex, remainingServices, slots, nextService);
                if (null == remainingPart) //musimy się wycofać
                {
                    remainingServices.Add(nextService.Code);
                }
                else
                {
                    List<ServiceVariantSlot> result = new List<ServiceVariantSlot>() { nextService };
                    result.AddRange(remainingPart);
                    return result;
                }
            }

            return null;
        }
    }
}
