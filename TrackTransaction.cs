using System;

namespace RacingTrackSolution2
{
    public class TrackTransaction
    {
        public EnumRaceTrackType RaceTrackType { get; set; }
 
        public EnumVehicleType VehicleType { get; set; }
        
        public string VehicleNumber { get; set; }
        
        public DateTime StartDateTime { get; set; }
        
        public DateTime EndDateTime { get; set; }
        
        public long Cost { get; set; }

        public void UpdateExitTime(TimeSpan time)
        {
            EndDateTime += time;
        }

        public bool IsWithinDuration(DateTime time)
        {
            if (time.TimeOfDay > StartDateTime.TimeOfDay && time.TimeOfDay < EndDateTime.TimeOfDay)
            {
                return true;
            }

            return false;
        }
    }
}