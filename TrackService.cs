using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RacingTrackSolution2
{
    public class TrackService : ITrackService
    {
        private DateTime BetweenFrom;
        private DateTime BetweenTo;

        private const string Success = "SUCCESS";
        private const string RaceTrackFull = "RACETRACK_FULL";
        private const string InvalidEntryTime = "INVALID_ENTRY_TIME";
        private const string InvalidExitTime = "INVALID_EXIT_TIME";

        private int extraCostPerHour = 50;
        private int _minutesPerHour = 60;
        private int _gracePeriod = 15;
        private TimeSpan _initialPeriod = new TimeSpan(3, 0, 0);

        public IList<RacingDetail> MasterRacingDetails = new List<RacingDetail>();
        public IList<TrackTransaction> TrackTransactionList = new List<TrackTransaction>();

        public TrackService(DateTime startTime, DateTime endTime)
        {
            SetTiming(startTime, endTime);
            LoadMasterData();
        }

        private void LoadMasterData()
        {
            MasterRacingDetails = File.ReadAllLines("TrackMaster.csv").Skip(1).Select(v => RacingDetail.FromCsv(v)).ToList();
        }

        private void SetTiming(DateTime startTime, DateTime endTime)
        {
            BetweenFrom = startTime;
            BetweenTo = endTime;
        }

        /// <summary>
        /// Validates entry and exit time 
        /// Check for track availablity
        /// adds the booking
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string BookRace(BookRaceRequest input)
        {
            //Validate entry time
            var entryTime = GetDate(input.EntryTime);
            var isValidTime = ValidateTimeOpenTimings(entryTime, true);
            if (!isValidTime)
            {
                return InvalidEntryTime;
            }

            // Validate exit time
            var exitTime = entryTime + new TimeSpan(3, 0, 0);
            isValidTime = ValidateTimeOpenTimings(exitTime);
            if (!isValidTime)
            {
                return InvalidExitTime;
            }

            //Check availablity of track
            var trackDetail = CheckRaceTrackAvailablity(input.VehicleType, entryTime, exitTime);

            if (trackDetail == null)
            {
                return RaceTrackFull;
            }

            //Add transaction
            AddTrackTransaction(input, entryTime, exitTime, trackDetail);
            return Success;
        }

        /// <summary>
        /// Validates the exit time
        /// Gets the books track 
        /// Updates the exit time for the existing booking
        /// validates the time
        /// </summary>
        /// <param name="input"><see cref="UpdateRaceRequest"/> with input data</param>
        /// <returns>return string with result</returns>
        public string UpdateRace(UpdateRaceRequest input)
        {
            var exitTime = GetDate(input.ExitTime);
            if (!ValidateTimeOpenTimings(exitTime))
            {
                return InvalidExitTime;
            }

            var track = TrackTransactionList.FirstOrDefault(item => item.VehicleNumber == input.VehicleNumber);

            track.UpdateExitTime(exitTime.TimeOfDay);
            return Success;
        }

        /// <summary>
        /// calculates the revenue for each transaction
        /// calculates total regular revenue 
        /// </summary>
        /// <returns>Returns string with regular and VIP data</returns>
        public string CalculateRevenue()
        {
            // Calculate the track cost for each item.
            foreach (var track in TrackTransactionList)
            {
                var racingDetail = MasterRacingDetails.FirstOrDefault(rd => rd.VehicleType == track.VehicleType && rd.RaceTrackType == track.RaceTrackType);
                track.Cost = CalculateFinalCost(track, racingDetail);
            }

            // Calculate total revenue generated.
            long regularRevenue = 0;
            long vipRevenue = 0;
            TrackTransactionList.Where(tt => tt.RaceTrackType == EnumRaceTrackType.REGULAR).ToList().ForEach(tt => { regularRevenue += tt.Cost; });
            TrackTransactionList.Where(tt => tt.RaceTrackType == EnumRaceTrackType.VIP).ToList().ForEach(tt => { vipRevenue += tt.Cost; });

            return $"{regularRevenue} {vipRevenue}";
        }

        private bool ValidateTimeOpenTimings(DateTime time, bool isStartTime = false)
        {
            var toTime = BetweenTo;

            if (isStartTime)
            {
                toTime = toTime.AddHours(-3);
            }
            if (time.TimeOfDay > BetweenFrom.TimeOfDay && time.TimeOfDay < toTime.TimeOfDay)
            {
                return true;
            }

            return false;
        }

        private DateTime GetDate(string strTime)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                            int.Parse(strTime.Split(':')[0]), int.Parse(strTime.Split(':')[1]), 0);
        }

        private void AddTrackTransaction(BookRaceRequest input, DateTime entryTime, DateTime exitTime, RacingDetail trackDetail)
        {
            var track = new TrackTransaction();
            track.RaceTrackType = trackDetail.RaceTrackType;
            track.VehicleNumber = input.VehicleNumber;
            track.VehicleType = input.VehicleType;
            track.StartDateTime = entryTime;
            track.EndDateTime = exitTime;

            TrackTransactionList.Add(track);
        }

        private long CalculateFinalCost(TrackTransaction track, RacingDetail TrackDetail)
        {
            //minimum cost is calculated
            var netCost = TrackDetail.CostPerHour * 3;
            //taken out 3 hours
            var time = track.EndDateTime - (track.StartDateTime + _initialPeriod);
            if (time.TotalMinutes > _gracePeriod)
            {
                // take out 15 min as its grace period
                var chargableMinutes = time.TotalMinutes - _gracePeriod;
                // count number of hours + extra minute counted as 1 hour 
                netCost += extraCostPerHour * (chargableMinutes / _minutesPerHour + chargableMinutes % _minutesPerHour > 0 ? 1 : 0);
            }

            return netCost;
        }

        private RacingDetail CheckRaceTrackAvailablity(EnumVehicleType vehicleType, DateTime entryTime, DateTime exitTime)
        {
            RacingDetail retValue = null;
            foreach (var trackDetail in MasterRacingDetails.Where(td => td.VehicleType == vehicleType && td.RaceTrackType == EnumRaceTrackType.REGULAR))
            {
                // vehicle type, race completed,
                var trackTransactionDetails = TrackTransactionList.Where(item => item.VehicleType == vehicleType && item.RaceTrackType == trackDetail.RaceTrackType && (item.IsWithinDuration(entryTime) || item.IsWithinDuration(exitTime))).ToList();
                if (trackTransactionDetails.Count < trackDetail.NoOfVehiclesAllowed)
                {
                    retValue = trackDetail;
                }
                else
                {
                    if (trackDetail.VehicleType == EnumVehicleType.CAR || trackDetail.VehicleType == EnumVehicleType.SUV)
                    {
                        trackTransactionDetails = TrackTransactionList.Where(item => item.VehicleType == vehicleType && item.RaceTrackType == EnumRaceTrackType.VIP && (item.IsWithinDuration(entryTime) || item.IsWithinDuration(exitTime))).ToList();
                        var vipTrackDetail = MasterRacingDetails.FirstOrDefault(item => item.VehicleType == vehicleType && item.RaceTrackType == EnumRaceTrackType.VIP);
                        if (trackTransactionDetails.Count < vipTrackDetail.NoOfVehiclesAllowed)
                        {
                            retValue = vipTrackDetail;
                        }
                    }
                }
            }
            return retValue;
        }

        public List<string> ExecuteRequest(string[] inputs)
        {
            var retValue = new List<string>();
            foreach (var v in inputs)
            {
                string result = "";
                var values = v.Split(',');
                if (values[0] == "BOOK")
                {
                    EnumVehicleType vehicleType;
                    Enum.TryParse<EnumVehicleType>(values[1], out vehicleType);

                    result = BookRace(new BookRaceRequest()
                    {
                        VehicleType = vehicleType,
                        VehicleNumber = values[2],
                        EntryTime = values[3]
                    });
                }
                else if (values[0] == "ADDITIONAL")
                {
                    result = UpdateRace(new UpdateRaceRequest()
                    {
                        VehicleNumber = values[1],
                        ExitTime = values[2]
                    });
                }
                else if (values[0] == "REVENUE")
                {
                    result = CalculateRevenue();
                }

                retValue.Add(result);
            }

            return retValue;
        }
    }
}