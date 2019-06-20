using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace RouteFinder
{
    class GoogleDistanceMetrixElementDistance
    {
        public String text;
        public Int32 value;
    }

    class GoogleDistanceMetrixElementDuration
    {
        public String text;
        public double value;
    }

    class GoogleDistanceMetrixElement
    {
        public GoogleDistanceMetrixElementDistance distance { get; set; }
        public GoogleDistanceMetrixElementDuration duration { get; set; }
        public String status { get; set; }
    }

    class GoogleDistanceMetrixRow
    {
        public GoogleDistanceMetrixElement[] elements { get; set; }
    }

    class GoogleDistanceMetrix
    {
        public String[] destination_addresses { get; set; }
        public String[] origin_addresses { get; set; }
        public GoogleDistanceMetrixRow[] rows { get; set; }
        public String status { get; set; }
    }

    internal class TransportationMap
    {
        private Dictionary<int, string> NumAddressDict = new Dictionary<int, string>();
        public static List<string> DeliveryPointsList = new List<string>();
        public List<double> TimeOfService = new List<double>();
        public Dictionary<Route, int> RoutesWithTimeList = new Dictionary<Route, int>();

        public TransportationMap()
        { }

        public TransportationMap(string pathToDB)
        {
            DeliveryPointsList.Clear();
            DeliveryPointsList.Add("проспект+50-річчя+СРСР+28А+Харків"); //address of Vodocanal
        }

        public void AddRequest(string pathToDB, AddRequestWindow wnd)
        {
            try
            {
                using (var sw = new StreamWriter(pathToDB, true))
                {
                    sw.WriteLine(wnd.NumberOfRequestBox.Text + "|"
                        + wnd.TypeOfStreetBox.Text + " "
                        + wnd.StreetNameBox.Text + " "
                        + wnd.NumberOfHouseBox.Text + " "
                        + wnd.CityBox.Text + "|" 
                        + wnd.TypeOfRequestBox.Text);
                }
            }
            catch(Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        public void FillDeliveryPointsList(string pathToDB)
        {
            try
            {
                using (var sr = new StreamReader(pathToDB, Encoding.GetEncoding(1251)))
                {
                    string containerForRead = " ";
                    while ((containerForRead = sr.ReadLine()) != null)
                    {
                        string pointOfDelivery = " ";
                        //NumAddressDict.Add(Int32.Parse(containerForRead.Split('|')[0]), containerForRead.Split('|')[3]);
                        for (int i = 0; i < containerForRead.Split('|')[1].Split(' ').Length; i++)
                        {
                            pointOfDelivery += containerForRead.Split('|')[1].Split(' ')[i] + '+';
                        }
                        
                        DeliveryPointsList.Add(pointOfDelivery);
                        TimeOfService.Add(TypeOfRequest(containerForRead.Split('|')[2]));
                    }
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private double TypeOfRequest(string type)
        {
            switch(type)
            {
                case "Пломбирование":
                    return 0.1;
                case "Проверка счетчика":
                    return 0.3;
                case "Замена счетчика":
                    return 1;
                case "Сан. Ремонт":
                    return 2;
                default:
                    return 0;
            }
        }

        private string CreateRequest(string departureAddress, string deliveryAddress)    //Creating url by elemets of DelivaryPointsList| Will call when mattrix will build
        {
            string URLofTarget = "https://maps.googleapis.com/maps/api/distancematrix/json?units=metric",
                   pointOfDeparture = "&origins=",
                   pointOfDelivery = "&destinations=",
                   keyGoogleAPI = "&key=AIzaSyAkD_hooUBpvTisFinjhY6CZEuEoemCKh8";

            URLofTarget += (pointOfDeparture + departureAddress)
                        + (pointOfDelivery + deliveryAddress)
                        + keyGoogleAPI;

            return URLofTarget;
        }

        public string CreateRequest(int routeChoise, List<Route> Routes)
        {
            string UrlOfTarget = "https://www.google.com.ua/maps/dir/" + DeliveryPointsList[0] +
                 '/' + DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[0].StartPoint] +
                '/' + DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[0].FinishPoint];
            object[] usedAddresses = new object[8];
            int usedAddressesIter = 0,
                i = (Routes[routeChoise].SectionsOfRouteList.Count > 1) ? 1 : 0;

            usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[0].StartPoint];
            usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[0].FinishPoint];
            if (i == 1)
            {
                do
                {
                    if (usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint])
                        && usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint]))
                    { }

                    else if (usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint]))
                    {
                        usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint];
                        usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint];
                        UrlOfTarget += '/' + DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint];
                    }

                    else if (usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint]))
                    {
                        usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint];
                        usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint];
                        UrlOfTarget += '/' + DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint];
                    }
                    i++;
                }
                while (i < Routes[routeChoise].SectionsOfRouteList.Count);
            }
            UrlOfTarget += '/' + DeliveryPointsList[0];
            return UrlOfTarget;
        }

        public double CalcTimeOfRoute(int routeChoise, List<Route> Routes)
        {
            double timeOfService = 0;
            object[] usedAddresses = new object[8];
            int usedAddressesIter = 0;

            for (int i = 0; i < Routes[routeChoise].SectionsOfRouteList.Count; i++)
            {
                timeOfService += (CallRequest(
                    DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint],
                    DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint]).duration.value / 360);

                if (usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint])
                        && usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint]))
                { }

                else if (usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint]))
                {
                    usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint];
                    usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint];
                    timeOfService += TimeOfService[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint] / 360;
                }

                else if (usedAddresses.Contains(DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint]))
                {
                    usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].StartPoint];
                    usedAddresses[usedAddressesIter++] = DeliveryPointsList[Routes[routeChoise].SectionsOfRouteList[i].FinishPoint];
                    timeOfService += TimeOfService[Routes[routeChoise].SectionsOfRouteList[i].StartPoint] / 360;
                }
            }
            return Math.Round(timeOfService, 2);
        }

        public GoogleDistanceMetrixElement CallRequest(string departureAddress, string deliveryAddress)
        {
            string URLofTarget = CreateRequest(deliveryAddress, departureAddress);
            
            WebRequest request = WebRequest.Create(URLofTarget);
            WebResponse response = request.GetResponse();

            Stream data = response.GetResponseStream();
            StreamReader reader = new StreamReader(data);

            var metr = JsonConvert.DeserializeObject<GoogleDistanceMetrix>(reader.ReadToEnd());
            try
            {
                return metr.rows[0].elements[0];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return default(GoogleDistanceMetrixElement);
            }

        }
    }

    internal class ClarkRightAlgorithm
    {
        TransportationMap transMapObj = new TransportationMap();
        private int[,] MatrixOfAdvantages = null;
        public static List<Route> RoutesList = new List<Route>();
        private List<Section> SectionList = new List<Section>();
        private Stack<Section> SectionStack = new Stack<Section>();
        private int matrixLenght = TransportationMap.DeliveryPointsList.Count;
        private List<Section> UsedPointsList = new List<Section>();


        public ClarkRightAlgorithm()
        { }
        public ClarkRightAlgorithm( int countCars)
        {
                RoutesList.Clear();
                SectionList.Clear();
                SectionStack.Clear();
                UsedPointsList.Clear();
                SetMatrixOfAdvantages();
                SectionLisBubbletSort();
                SetRoute(countCars);
        }

        public void SetMatrixOfAdvantages()
        {
            MatrixOfAdvantages = new int[matrixLenght, matrixLenght];


            for (int i = 0; i < matrixLenght; i++)
            {
                for (int j = 1; j < matrixLenght; j++)
                {
                    if (i != j)
                    {
                        if (i >= j && j > 0)
                        {
                            MatrixOfAdvantages[i, j] = MatrixOfAdvantages[0, i]
                                + MatrixOfAdvantages[0, j]
                                - transMapObj.CallRequest(TransportationMap.DeliveryPointsList[i], TransportationMap.DeliveryPointsList[j]).distance.value;

                            SectionList.Add(new Section() { StartPoint = i, FinishPoint = j, KmAdvantage = MatrixOfAdvantages[i, j]});

                        }
                        else
                        {
                            MatrixOfAdvantages[i, j] = transMapObj.CallRequest(TransportationMap.DeliveryPointsList[i], TransportationMap.DeliveryPointsList[j]).distance.value;
                        }
                    }
                }
            }
        }

        private void SetRoute(int countCars)
        {
            int[] usedPoints = new int[36];
            int iteratorRoutes = 100;

            SectionLisBubbletSort();
            Copy();

            for (int i = 0; i < iteratorRoutes; i++)
            {
                int usedPointIndex = 0;
                try
                {
                    if (!SectionStack.Peek().IsSectionUsed && RoutesList.Count < countCars)
                    {
                        RoutesList.Add(new Route(SectionStack.Pop()));
                        usedPoints[usedPointIndex++] = RoutesList[RoutesList.Count - 1].FirstPoint;
                        usedPoints[usedPointIndex++] = RoutesList[RoutesList.Count - 1].LastPoint;
                        while (RoutesList[RoutesList.Count - 1].SectionsOfRouteList.Count != 3)
                        {
                            if (!SectionStack.Peek().IsSectionUsed && !TripleUsedPoint(SectionStack.Peek().StartPoint, usedPoints)
                               && !TripleUsedPoint(SectionStack.Peek().FinishPoint, usedPoints)
                               && !(RoutesList[RoutesList.Count - 1].FirstPoint == SectionStack.Peek().FinishPoint && RoutesList[RoutesList.Count - 1].LastPoint == SectionStack.Peek().StartPoint))
                            {
                                if (SectionStack.Peek().StartPoint == RoutesList[RoutesList.Count - 1].FirstPoint)
                                {
                                    usedPoints[usedPointIndex++] = RoutesList[RoutesList.Count - 1].FirstPoint;
                                    usedPoints[usedPointIndex++] = RoutesList[RoutesList.Count - 1].FirstPoint = SectionStack.Peek().FinishPoint;
                                    Swap(SectionStack.Peek());
                                    RoutesList[RoutesList.Count-1].SectionsOfRouteList.Insert(0, SectionStack.Pop());
                                }
                                else if (SectionStack.Peek().StartPoint == RoutesList[RoutesList.Count - 1].LastPoint)
                                {
                                    usedPoints[usedPointIndex++] = RoutesList[RoutesList.Count - 1].LastPoint;
                                    usedPoints[usedPointIndex++] = RoutesList[RoutesList.Count - 1].LastPoint = SectionStack.Peek().FinishPoint;
                                    RoutesList[RoutesList.Count - 1].SectionsOfRouteList.Add(SectionStack.Pop());
                                }
                                else break;
                            }
                            else
                            { SectionStack.Pop(); }
                        }
                        foreach (Section deadPoint in SectionStack)
                        {
                            if (usedPoints.Contains(deadPoint.StartPoint) || usedPoints.Contains(deadPoint.FinishPoint))
                            {
                                deadPoint.IsSectionUsed = true;
                            }
                        }
                    }
                    else SectionStack.Pop();

                }
                catch (Exception ex)
                {
                    i = iteratorRoutes;
                    MessageBox.Show("Маршруты построены");
                }
            }
        }

        private void SectionLisBubbletSort()
        {
            Section tmp;
            for (int i = 0; i < SectionList.Count; i++)
            {
                for (int j = 0; j < SectionList.Count; j++)
                {
                    if (SectionList[i].KmAdvantage < SectionList[j].KmAdvantage)
                    {
                        tmp = SectionList[i];
                        SectionList[i] = SectionList[j];
                        SectionList[j] = tmp;
                    }
                }
            }
        }

        private void Swap(Section secObj)
        {
            int tmp = secObj.StartPoint;
            secObj.StartPoint = secObj.FinishPoint;
            secObj.FinishPoint = tmp;
        }

        private void Copy()
        {
            for (int i = 0; i < SectionList.Count; i++)
            {
                SectionStack.Push(SectionList[i]);
            }
        }

        private bool TripleUsedPoint(int inputNumber, int[] usedPoints)
        {
            int callCounter = 0;
            for (int i = 0; i < usedPoints.Length; i++)
            {
                if (usedPoints[i] == inputNumber)
                {
                    callCounter++;
                    if (callCounter >= 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string RouteRequest(int index)
        {
            return transMapObj.CreateRequest(index, RoutesList);
        }
    }

    internal class Route
    {
        int firstPoint,
            lastPoint,
            workerCounter;

        public int FirstPoint
        {
            set { firstPoint = value; }
            get { return firstPoint; }
        }

        public int LastPoint
        {
            set { lastPoint = value; }
            get { return lastPoint; }
        }

        public List<Section> SectionsOfRouteList = new List<Section>();

        public Route()
        {
            firstPoint = 0;
            lastPoint = 0;
            workerCounter = 0;
        }
        public Route(Section section)
        {
            SectionsOfRouteList.Add(section);
            firstPoint = section.StartPoint;
            lastPoint = section.FinishPoint;
            workerCounter += 4;
        }

        public static Route operator +(Route obj1, Route obj2)
        {
            obj1.SectionsOfRouteList.Concat(obj2.SectionsOfRouteList);
            if (obj2.lastPoint == obj1.lastPoint)
            {
                obj1.lastPoint = obj2.firstPoint;
            }
            obj1.workerCounter += obj2.workerCounter;
            return new Route();
        }

        private void Swap(Section obj1, Section obj2)
        {
            int tmp = obj1.StartPoint;
            obj1.FinishPoint = obj2.StartPoint;
            obj2.StartPoint = tmp;
        }

    }

    public class Section
    {
        private int startPoint,
                   finishPoint,
                   kmAdvantage;
        private bool isSectionUsed = false;


        public int StartPoint
        {
            set { startPoint = value; }
            get { return startPoint; }
        }

        public int FinishPoint
        {
            set { finishPoint = value; }
            get { return finishPoint; }
        }

        public int KmAdvantage
        {
            set { kmAdvantage = value; }
            get { return kmAdvantage; }
        }

        public bool IsSectionUsed
        {
            set { isSectionUsed = value; }
            get { return isSectionUsed; }
        }
    }
}
