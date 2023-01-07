using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingTrackSolution2
{
    public interface ITrackService
    {
        /// <summary>
        /// Takes the file inputs to execute
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        List<string> ExecuteRequest(string[] inputs);

        /// <summary>
        /// Validates entry and exit time 
        /// Check for track availablity
        /// adds the booking
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Returns the status or the error messages</returns>
        string BookRace(BookRaceRequest input);

        /// <summary>
        /// Validates the exit time
        /// Gets the books track 
        /// Updates the exit time for the existing booking
        /// validates the time
        /// </summary>
        /// <param name="input"><see cref="UpdateRaceRequest"/> with input data</param>
        /// <returns>return string with result</returns>
        string UpdateRace(UpdateRaceRequest input);

        /// <summary>
        /// calculates the revenue for each transaction
        /// calculates total regular revenue 
        /// </summary>
        /// <returns>Returns string with regular and VIP data</returns>
        string CalculateRevenue();
    }
}
