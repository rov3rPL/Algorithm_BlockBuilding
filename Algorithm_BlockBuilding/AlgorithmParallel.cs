//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using MethodTimer;

//namespace Algorithm_BlockBuilder
//{
//    internal class AlgorithmParallel
//    {
//        private readonly int MAX_OFFSET_BETWEEN_SERVICES;

//        public AlgorithmParallel(int MAX_OFFSET_BETWEEN_SERVICES)
//        {
//            this.MAX_OFFSET_BETWEEN_SERVICES = MAX_OFFSET_BETWEEN_SERVICES;

//        }

//        [Time]
//        public List<Block> Run(ServiceVariantSlot[] slots)
//        {
//            HashSet<char> allServices = new HashSet<char>();
//            for (int i = 0; i < slots.Length; ++i)
//                allServices.Add(slots[i].Code);

//            var matrix = GenerateMatrix(slots);
//            //var result = Start(matrix, allServices, slots);
//            var result = StartParallel(matrix, allServices, slots); //worse?
//            return result;
//        }

//        //TODO: makt it parallel
//        private int?[][] GenerateMatrix(ServiceVariantSlot[] slots)
//        {
//            //TODO:
//            //slots - posortowane po czasie rozpoczęcia
//            int?[][] matrix = new int?[slots.Length - 1][];
//            for (int i = 0; i < matrix.Length; ++i)
//            {
//                matrix[i] = new int?[matrix.Length - i];
//                for (int j = 0; j < matrix.Length - i; ++j)
//                {
//                    var SV1 = slots[i];
//                    var SV2 = slots[i + j + 1];
//                    //if (SV1.Code == SV2.Code) //te same warianty
//                    //    matrix[i][j] = null;
//                    var timeOffset = SV2.StartTime - SV1.EndTime;
//                    //if(timeOffset > maxTimeOffset)
//                    //    matrix[i][j] = null;
//                    if (SV1.Code != SV2.Code && timeOffset >= 0)
//                        matrix[i][j] = timeOffset;
//                }
//            }

//            return matrix;
//        }

//        List<Block> StartParallel(int?[][] matrix, HashSet<char> allServices, ServiceVariantSlot[] slots)
//        {
//            ConcurrentBag<Block> listOfBlocks = new ConcurrentBag<Block>();

//            Parallel.For(0, matrix.Length, b =>
//            {
//                var remainingServices = new HashSet<char>(allServices);
//                var firstService = slots[b];
//                remainingServices.Remove(firstService.Code);
//                var remainingPart = SearchParallel(matrix, b, remainingServices, slots, firstService);
//                if (remainingServices.Count == 0)
//                {
//                    var result = new List<ServiceVariantSlot>() { firstService };
//                    result.AddRange(remainingPart);
//                    var block = new Block();
//                    block.ServiceVariantSlots = result.ToArray();
//                    listOfBlocks.Add(block);
//                }
//            });

//            return listOfBlocks.Count > 0 ? listOfBlocks.ToList() : null;
//        }

//        private ConcurrentBag<ServiceVariantSlot> SearchParallel(int?[][] matrix, int startIndex, ConcurrentDictionary<char, bool> remainingServices, ServiceVariantSlot[] slots, ServiceVariantSlot previousService)
//        {
//            //concurrent - HashSet<char> remainingServices
//            ConcurrentDictionary<char, bool> remainingServicesCopy = new ConcurrentDictionary<char, bool>(remainingServices);

//            //concurrent result
//            ConcurrentBag<ServiceVariantSlot> result = new ConcurrentBag<ServiceVariantSlot>();

//            Parallel.For(0, matrix[startIndex].Length, (j, loopState) => {
//                if (remainingServicesCopy.Count == 0)
//                    loopState.Stop(); // ? //return null;

//                if (matrix[startIndex][j] == null)
//                    return; //continue;
//                var nextServiceIndex = startIndex + j + 1;
//                var nextService = slots[nextServiceIndex];
//                if (!remainingServicesCopy[nextService.Code]) //ta usługa jest już dobrana
//                    return; //continue;
//                if (nextService.StartTime < previousService.EndTime) //nie bierz, jeżeli slot zachodzi na ostatni wybrany slot
//                    return; //continue;                    //TODO: max interval - here or while generating matrix ?
//                if (previousService.EndTime + MAX_OFFSET_BETWEEN_SERVICES < nextService.StartTime)
//                    return; //continue;

//                remainingServicesCopy[nextService.Code] = false;
//                if (remainingServicesCopy.Count == 0)
//                {
//                    return new List<ServiceVariantSlot>() { nextService };
//                }
//                if (nextServiceIndex >= matrix.Length) //są jeszcze usługi do umówienia a nie ma już slotów
//                {
//                    remainingServicesCopy[nextService.Code] = true;
//                    loopState.Stop(); // ? //return null;
//                }
//                var remainingPart = SearchParallel(matrix, nextServiceIndex, remainingServicesCopy, slots, nextService);
//                if (null == remainingPart) //musimy się wycofać
//                {
//                    remainingServicesCopy[nextService.Code] = true;
//                }
//                else
//                {
//                    List<ServiceVariantSlot> result = new List<ServiceVariantSlot>() { nextService };
//                    result.AddRange(remainingPart);
//                    return result;
//                }
//            });
                       

//            return null;
//        }
//    }
//}
