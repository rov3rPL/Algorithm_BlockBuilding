using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xline.IO;

namespace Algorithm_BlockBuilder
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static /*readonly*/ int MAX_OFFSET_BETWEEN_SERVICES
            = 30;
            //= 15;
            //= 60;


        static int CalculateNumberOfMinutes(int hours, int minutes = 0)
        {
            return 60 * hours + minutes;
        }

        static string NumberOfMinutesToHumanTime(int minutes)
        {
            return new TimeSpan(0, minutes, 0).ToString("hh\\:mm");
        }

        internal class CSVData
        {
            public string Code { get; set; }
            public dynamic StartTimeHour { get; set; }
            public dynamic StartTimeMinutes { get; set; }
            public dynamic EndTimeHour { get; set; }
            public dynamic EndTimeMinutes { get; set; }
        }

        public static ServiceVariantSlot CSVDataToSlot(CSVData data)
        {
            return new ServiceVariantSlot()
            {
                Code = data.Code[0],
                StartTime = CalculateNumberOfMinutes(
                    Convert.ToInt32(data.StartTimeHour),
                    Convert.ToInt32(data.StartTimeMinutes)
                ),
                EndTime = CalculateNumberOfMinutes(
                    Convert.ToInt32(data.EndTimeHour),
                    Convert.ToInt32(data.EndTimeMinutes)
                )
            };
        }

        public static string PrintServiceVariantSlot(ServiceVariantSlot slot)
        {
            return String.Format("{0} {1} {2}", slot.Code, NumberOfMinutesToHumanTime(slot.StartTime), NumberOfMinutesToHumanTime(slot.EndTime));
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var args_MAX_OFFSET_BETWEEN_SERVICES = Convert.ToInt32(args[0]);
                MAX_OFFSET_BETWEEN_SERVICES = args_MAX_OFFSET_BETWEEN_SERVICES;
            }
            log.Info("[START]");            

            var reader = new DelimitedFileReader(@"input.csv", 0, 0, ',');
            reader.SkipEmptyRows = true;
            List<CSVData> list = reader.ToList<CSVData>();
            list.Remove(list.FindLast(x => true)); //trial thing?
            var slots = list.Select(CSVDataToSlot).ToArray();


            //HashSet<char> allServices = new HashSet<char>() { 'L', 'T', 'P' };
            //ServiceVariantSlot[] slots = new ServiceVariantSlot[]
            //{
            //    new ServiceVariantSlot()
            //    {
            //        Code = 'L',
            //        StartTime = CalculateNumberOfMinutes(13),
            //        EndTime = CalculateNumberOfMinutes(13,30)
            //    },
            //    new ServiceVariantSlot()
            //    {
            //        Code = 'T',
            //        StartTime = CalculateNumberOfMinutes(13),
            //        EndTime = CalculateNumberOfMinutes(13,30)
            //    },
            //    new ServiceVariantSlot()
            //    {
            //        Code = 'L',
            //        StartTime = CalculateNumberOfMinutes(13,30),
            //        EndTime = CalculateNumberOfMinutes(14)
            //    },
            //    new ServiceVariantSlot()
            //    {
            //        Code = 'L',
            //        StartTime = CalculateNumberOfMinutes(14),
            //        EndTime = CalculateNumberOfMinutes(14,30)
            //    },
            //    new ServiceVariantSlot()
            //    {
            //        Code = 'T',
            //        StartTime = CalculateNumberOfMinutes(14,30),
            //        EndTime = CalculateNumberOfMinutes(15)
            //    },
            //    new ServiceVariantSlot()
            //    {
            //        Code = 'P',
            //        StartTime = CalculateNumberOfMinutes(15),
            //        EndTime = CalculateNumberOfMinutes(15,20)
            //    },
            //    new ServiceVariantSlot()
            //    {
            //        Code = 'L',
            //        StartTime = CalculateNumberOfMinutes(15),
            //        EndTime = CalculateNumberOfMinutes(15,30)
            //    }
            //};

            //int?[][] M = new int?[6][]
            //{
            //    new int?[] {null, null, null, 60, 90, null},
            //    new int?[] {null, 30, null, 90, 90},
            //    new int?[] {null, 30, 60, null},
            //    new int?[] {null, 30, null},
            //    new int?[] {0, 0},
            //    new int?[] {null}
            //};

            //var a = new Program();
            //var matrix = a.GenerateMatrix(slots); // == M
            ////var R = a.Run(matrix, allServices, slots);

            //var R = a.RunRecursive(matrix, allServices, slots);

            Array.Sort(slots, delegate(ServiceVariantSlot s1, ServiceVariantSlot s2)
                                { return s1.StartTime - s2.StartTime; });
            var alg = new Algorithm(MAX_OFFSET_BETWEEN_SERVICES);

            log.Info(String.Format("MAX_OFFSET_BETWEEN_SERVICES = {0}", MAX_OFFSET_BETWEEN_SERVICES));
            
            var result = alg.Run(slots);


            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            if (null != result)
                result.OrderBy(b => b.SpareTime).ToList().ForEach(block => {
                    stringBuilder.AppendLine($"BLOK (przerwy sumarycznie - {block.SpareTime} minut):");
                    block.ServiceVariantSlots.ToList().ForEach(slot =>
                        stringBuilder.AppendLine(PrintServiceVariantSlot(slot)));
                });
            else
                stringBuilder.AppendLine("BRAK WYNIKU");

            log.Info(stringBuilder.ToString());

        }
        //struct ServiceVariantSlot
        //{
        //    internal char Code;
        //    internal int StartTime;
        //    internal int EndTime;
        //}

        Stack<ServiceVariantSlot> Run(int?[][] matrix, HashSet<char> allServices, ServiceVariantSlot[] slots)
        {

            var result = new Stack<ServiceVariantSlot>(allServices.Count);
            for (int i = 0; i < matrix.Length; ++i)
            {
                var selectedServices = new Stack<ServiceVariantSlot>(allServices.Count);
                var remainingServices = new HashSet<char>(allServices);
                var firstService = slots[i];
                remainingServices.Remove(firstService.Code);
                selectedServices.Push(firstService); //pierwsza usługa w bloku
                for (int j = 0; j < matrix[i].Length && remainingServices.Count > 0; ++j)
                {
                    if (matrix[i][j] == null)
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

        List<ServiceVariantSlot> RunRecursive(int?[][] matrix, HashSet<char> allServices, ServiceVariantSlot[] slots)
        {            
            for (int b = 0; b < matrix.Length; ++b)
            {
                var remainingServices = new HashSet<char>(allServices);
                var firstService = slots[b];
                remainingServices.Remove(firstService.Code);
                var remainingPart = search(matrix, b, remainingServices, slots, firstService);
                if (remainingServices.Count == 0)
                {
                    var result = new List<ServiceVariantSlot>() { firstService };
                    result.AddRange(remainingPart);
                    return result;
                }                    
            }

            return null;
        }

        List<ServiceVariantSlot> search(int?[][] matrix, int startIndex, HashSet<char> remainingServices, ServiceVariantSlot[] slots, ServiceVariantSlot previousService)
        {
            for (int j = 0; j < matrix[startIndex].Length && remainingServices.Count > 0; ++j)
            {
                if (matrix[startIndex][j] == null)
                    continue;
                var nextService = slots[startIndex + j + 1];
                if (!remainingServices.Contains(nextService.Code)) //ta usługa jest już dobrana
                    continue;
                if (nextService.StartTime < previousService.EndTime) //nie bierz, jeżeli slot zachodzi na ostatni wybrany slot
                    continue;                                           //TODO: max interval - here or while generating matrix ?
                if (previousService.EndTime + MAX_OFFSET_BETWEEN_SERVICES < nextService.StartTime)
                    continue;

                remainingServices.Remove(nextService.Code);
                if(remainingServices.Count == 0)
                {
                    return new List<ServiceVariantSlot>() { nextService };
                }
                ///
                var remainingPart = search(matrix, startIndex + j + 1, remainingServices, slots, nextService);
                if(null == remainingPart) //musimy się wycofać
                {
                    remainingServices.Add(nextService.Code);
                }
                else
                {
                    List<ServiceVariantSlot> result = new List<ServiceVariantSlot>() { nextService };
                    result.AddRange(remainingPart);
                    return result;
                }
                ///
                ////PROBA #2
                //result = new List<ServiceVariantSlot>() { nextService }; //dobieramy kolejną usługę w bloku
                //if (remainingServices.Count == 0)
                //    return result;        //pełny blok
                //var remainingPart = search(matrix, startIndex + 1, remainingServices, slots);
                //if(null == remainingPart) //musimy się wycofać
                //{
                //    remainingServices.Add(nextService.Code);
                //    continue;
                //}
                //result.AddRange(remainingPart);


                ////PROBA #1
                //remainingServices.Remove(nextService.Code);

                //result = search(matrix, startIndex+1, remainingServices, slots);
                //result.Add(nextService); //dobieramy kolejną usługę w bloku

                //if (remainingServices.Count == 0)
                //    return result;        //pełny blok
            }

            return null;
        }
        

        Stack<ServiceVariantSlot> Ruuun(int?[][] matrix, HashSet<char> allServices, ServiceVariantSlot[] slots)
        {
            for (int b = 0; b < matrix.Length; ++b) //b - begin, początek bloku
            {

                var selectedServices = new Stack<ServiceVariantSlot>(allServices.Count);
                var remainingServices = new HashSet<char>(allServices);
                var firstService = slots[b];

                remainingServices.Remove(firstService.Code);
                selectedServices.Push(firstService); //pierwsza usługa w bloku
                if (remainingServices.Count == 0)
                    return selectedServices;        //pełny blok

                for (int j = 0; j < matrix[b].Length && remainingServices.Count > 0; ++j)
                {
                    if (matrix[b][j] == null)
                        continue;
                    var nextService = slots[j + b + 1];
                    if (!remainingServices.Contains(nextService.Code)) //ta usługa jest już dobrana
                        continue;
                    if (nextService.StartTime < selectedServices.Peek().EndTime) //nie bierz, jeżeli slot zachodzi na ostatni wybrany slot
                        continue;                                           //TODO: max interval - here or while generating matrix ?

                    remainingServices.Remove(nextService.Code);
                    selectedServices.Push(nextService); //dobieramy kolejną usługę w bloku
                    if (remainingServices.Count == 0)
                        return selectedServices;        //pełny blok

                    // ~
                    b += j; // nowym początkiem jest
                }

            }


            for (int i = 0; i < matrix.Length; ++i)
            {
                var selectedServices = new Stack<ServiceVariantSlot>(allServices.Count);
                var remainingServices = new HashSet<char>(allServices);
                var firstService = slots[i];
                remainingServices.Remove(firstService.Code);
                selectedServices.Push(firstService); //pierwsza usługa w bloku

                for (int j = 0; j < matrix[i].Length && remainingServices.Count > 0; ++j)
                {
                    if (matrix[i][j] == null)
                        continue;
                    var nextService = slots[j + i + 1];
                    if (!remainingServices.Contains(nextService.Code)) //ta usługa jest już dobrana
                        continue;
                    if (nextService.StartTime < selectedServices.Peek().EndTime) //nie bierz, jeżeli slot zachodzi na ostatni wybrany slot
                        continue;

                    remainingServices.Remove(nextService.Code);
                    selectedServices.Push(nextService); //dobieramy kolejną usługę w bloku
                    if (remainingServices.Count == 0)
                        return selectedServices;

                    ++i;
                    break;
                }
                if (remainingServices.Count > 0)
                {

                }

            }
            return null;
        }

        int?[][] GenerateMatrix(ServiceVariantSlot[] slots)
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
                    if (SV1.Code != SV2.Code && timeOffset >= 0)
                        matrix[i][j] = timeOffset;
                }
            }

            return matrix;
        }

        //int?[][] GenerateMatrix(ServiceVariantSlot[] slots, int maxTimeOffset = 30)
        //{
        //    //TODO:
        //    //slots - posortowane po czasie rozpoczęcia
        //    int?[][] matrix = new int?[slots.Length - 1][];
        //    for (int i = 0; i < matrix.Length; ++i)
        //    {
        //        matrix[i] = new int?[matrix.Length - i];
        //        for (int j = 0; j < matrix.Length - i; ++j)
        //        {
        //            var SV1 = slots[i];
        //            var SV2 = slots[i + j + 1];
        //            //if (SV1.Code == SV2.Code) //te same warianty
        //            //    matrix[i][j] = null;
        //            var timeOffset = SV2.StartTime - SV1.EndTime;
        //            //if(timeOffset > maxTimeOffset)
        //            //    matrix[i][j] = null;
        //            if (SV1.Code != SV2.Code && timeOffset >= maxTimeOffset /* >= 0 */)
        //                matrix[i][j] = timeOffset;
        //        }
        //    }

        //    return matrix;
        //}
    }

}