using System.Collections.Generic;

namespace Algorithm_BlockBuilder
{
    class ProgramOLD
    {

        static void MainZZZ(string[] args)
        {
            int?[][] M = new int?[7][]
            {
                new int?[] {null, null, null, null, 60, 90, null},
                new int?[] {null, 45, 75, 75, null, 105},
                new int?[] {null, null, 0, 0, null},
                new int?[] {null, null, 0, null},
                new int?[] {null, null, 10},
                new int?[] {null, 15},
                new int?[] {null}
            };
            HashSet<char> allServices = new HashSet<char>() { 'L', 'T', 'P' };
            ServiceVariantSlot[] slots = new ServiceVariantSlot[]
            {
                new ServiceVariantSlot()
                {
                    Code = 'L',
                    StartTime = 1300,
                    EndTime = 1330
                },
                new ServiceVariantSlot()
                {
                    Code = 'T',
                    StartTime = 1300,
                    EndTime = 1315
                },
                new ServiceVariantSlot()
                {
                    Code = 'L',
                    StartTime = 1400,
                    EndTime = 1430
                },
                new ServiceVariantSlot()
                {
                    Code = 'L',
                    StartTime = 1430,
                    EndTime = 1500
                },
                new ServiceVariantSlot()
                {
                    Code = 'P',
                    StartTime = 1430,
                    EndTime = 1450
                },
                new ServiceVariantSlot()
                {
                    Code = 'T',
                    StartTime = 1430,
                    EndTime = 1445
                },
                new ServiceVariantSlot()
                {
                    Code = 'L',
                    StartTime = 1500,
                    EndTime = 1530
                }
            };

            //var R = new Program().Run(M, allServices, slots);

            int?[][] M2 = new int?[6][]
            {
                new int?[] {null, null, null, 60, 60, null},
                new int?[] {45, 75, 75, null, 105},
                new int?[] {null, 0, 0, null},
                new int?[] {null, null, null},
                new int?[] {null, 10},
                new int?[] {15}
            };
            var a = new ProgramOLD();
            var matrix = a.GenerateMatrix(slots);
            var R2 = a.Run2(M2, allServices, slots);
        }
        struct ServiceVariantSlot
        {
            internal char Code;
            internal int StartTime;
            internal int EndTime;
        }
        //Stack<ServiceVariantSlot> Run(int?[][] M, HashSet<char> allServices, ServiceVariantSlot[] slots) {
        //    var result = new Stack<ServiceVariantSlot>(allServices.Count);
        //    var servicesHash = new HashSet<char>(allServices);
        //    for (int i=0; i < M.Length && servicesHash.Count > 0; ++i)
        //    {
        //        var SV1 = slots[i];
        //        servicesHash.Remove(SV1.Code);
        //        result.Push(SV1);
        //        for(int j=0; j < M[i].Length && servicesHash.Count > 0; ++j)
        //        {
        //            if (M[i][j] == null) //first run is diagonal, always null, change 'matrix' and indexes
        //                continue;
        //            var SV2 = slots[j + i]; //slots[j + i + 1];
        //            if (SV2.StartTime < result.Peek().EndTime) //czy zachodzą
        //                continue;
        //            if (!servicesHash.Contains(SV2.Code))
        //                continue;
        //            servicesHash.Remove(SV2.Code);
        //            result.Push(SV2);
        //        }
        //        if(servicesHash.Count>0)
        //        {
        //            result = new Stack<ServiceVariantSlot>();
        //            //result.Pop(); //remove SV1
        //            servicesHash = new HashSet<char>(allServices);
        //        }
        //    }
        //    return result;
        //}

        Stack<ServiceVariantSlot> Run2(int?[][] M, HashSet<char> allServices, ServiceVariantSlot[] slots)
        {
            //var result = new Stack<ServiceVariantSlot>(allServices.Count);
            //var remainingServices = new HashSet<char>(allServices);
            //for (int i = 0; i < M.Length && remainingServices.Count > 0; ++i)
            //{
            //    var SV1 = slots[i];
            //    remainingServices.Remove(SV1.Code);
            //    result.Push(SV1);
            //    for (int j = 0; j < M[i].Length && remainingServices.Count > 0; ++j)
            //    {
            //        if (M[i][j] == null)
            //            continue;
            //        var SV2 = slots[j + i]; //slots[j + i + 1];
            //        if (!remainingServices.Contains(SV2.Code)) //ta usługa jest już dobrana
            //            continue;
            //        if (SV2.StartTime < result.Peek().EndTime) //nie bierz, jeżeli slot zachodzi na ostatni wybrany slot
            //            continue;
            //        remainingServices.Remove(SV2.Code);
            //        result.Push(SV2);
            //    }
            //    if (remainingServices.Count > 0) //reset z nowym początkiem
            //    {
            //        result = new Stack<ServiceVariantSlot>();
            //        //result.Pop(); //remove SV1
            //        remainingServices = new HashSet<char>(allServices);
            //    }
            //}
            //ALTERNATYWA
            var result = new Stack<ServiceVariantSlot>(allServices.Count);
            for (int i = 0; i < M.Length; ++i)
            {
                var selectedServices = new Stack<ServiceVariantSlot>(allServices.Count);
                var remainingServices = new HashSet<char>(allServices);
                var firstService = slots[i];
                remainingServices.Remove(firstService.Code);
                selectedServices.Push(firstService); //pierwsza usługa w bloku
                for (int j = 0; j < M[i].Length && remainingServices.Count > 0; ++j)
                {
                    if (M[i][j] == null)
                        continue;
                    var nextService = slots[j + i + 1];
                    if (!remainingServices.Contains(nextService.Code)) //ta usługa jest już dobrana
                        continue;
                    if (nextService.StartTime < selectedServices.Peek().EndTime) //nie bierz, jeżeli slot zachodzi na ostatni wybrany slot
                        continue;
                    remainingServices.Remove(nextService.Code);
                    selectedServices.Push(nextService); //dobieramy kolejną usługę w bloku
                }
                if (remainingServices.Count == 0)
                { //wybrany został pełny zestaw usług
                    result = selectedServices;
                    break;
                }
            }
            return result;
        }
        int?[][] GenerateMatrix(ServiceVariantSlot[] slots, int maxTimeOffset = 30)
        {
            //TODO:
            //slots - posortowane po czasie rozpoczęcia
            int?[][] matrix = new int?[slots.Length - 1][];
            for (int i = 0; i < matrix.Length; ++i)
            {
                matrix[i] = new int?[matrix.Length - i];
                for (int j = 0; j < matrix.Length - i; ++j)
                {
                    var SV1 = slots[i];
                    var SV2 = slots[i + j + 1];
                    //if (SV1.Code == SV2.Code) //te same warianty
                    //    matrix[i][j] = null;
                    var timeOffset = SV2.StartTime - SV1.EndTime;
                    //if(timeOffset > maxTimeOffset)
                    //    matrix[i][j] = null;
                    if (SV1.Code != SV2.Code && timeOffset > maxTimeOffset /* > 0 */)
                        matrix[i][j] = timeOffset;
                }
            }

            return matrix;
        }
    }

}