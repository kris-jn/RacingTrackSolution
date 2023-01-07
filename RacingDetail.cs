using System;
using System.Collections.Generic;
using System.Linq;

namespace RacingTrackSolution2
{
    public class RacingDetail
    {
        public EnumRaceTrackType RaceTrackType { get; set; }
        public EnumVehicleType VehicleType { get; set; }
        public int NoOfVehiclesAllowed { get; set; }
        public long CostPerHour { get; set; }

        public static RacingDetail FromCsv(string csvLine)
        {
            var racingDetails = new RacingDetail();
            var values = csvLine.Split(',');
            foreach (var value in values)
            {
                EnumRaceTrackType raceTrackType;
                Enum.TryParse<EnumRaceTrackType>(values[0], out raceTrackType);
                EnumVehicleType vehicleType;
                Enum.TryParse<EnumVehicleType>(values[1], out vehicleType);

                racingDetails.RaceTrackType = raceTrackType;
                racingDetails.VehicleType= vehicleType;
                racingDetails.NoOfVehiclesAllowed = int.Parse(values[2]);
                racingDetails.CostPerHour = int.Parse(values[3]);
            }
            return racingDetails;
        }
    }

}