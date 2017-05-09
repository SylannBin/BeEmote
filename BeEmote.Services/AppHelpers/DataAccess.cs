﻿using BeEmote.Core;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BeEmote.Services
{
    /// <summary>
    /// Contains all the logic of working with the database.
    /// Does not take care of the code,
    /// Does not know about procedures (relies on DBManager).
    /// </summary>
    public class DataAccess
    {
        /// <summary>
        /// Executes the stored procedure insertinto_imganalysis
        /// </summary>
        public List<int> GetImgAnalysis()
        {
            // Keeps the connection open only to execute the logique and close it immediately after
            using (IDbConnection conn = new MySqlConnection(DatabaseManager.MySql_BeEmote))
            {
                return conn.Query<int>(DatabaseManager.SelectFromImgAnalysis, new int()).ToList(); // Dapper syntax
            }
        }

        /// <summary>
        /// Executes the stored procedure insertinto_imganalysis.
        /// Instanciates a new list (the insert method needs a list),
        /// and immediately fills it with a new int
        /// </summary>
        public void InsertImgAnalysis()
        {
            JsonManager Json = new JsonManager();
            string jsonResponse = JsonExamples.GetEmotionAPIResult2();
            List<Face> Faces = Json.GetFacesFromJson(jsonResponse);

            using (IDbConnection conn = new MySqlConnection(DatabaseManager.MySql_BeEmote))
            {
                int IdImg = conn.Query<int>(DatabaseManager.InsertIntoImgAnalysis, new { NbFaces = Faces.Count }).Single();
                foreach (Face f in Faces)
                {
                    var Dominant = f.GetDominantEmotion();
                    conn.Execute(DatabaseManager.InsertIntoEmotion, new
                    {
                        IdImg,
                        Dominant,
                        RLeft = f.FaceRectangle.Left,
                        RTop = f.FaceRectangle.Top,
                        RWidth = f.FaceRectangle.Width,
                        RHeight = f.FaceRectangle.Height,
                        Anger = f.Scores.Anger,
                        Contempt = f.Scores.Contempt,
                        Disgust = f.Scores.Disgust,
                        Fear = f.Scores.Fear,
                        Happiness = f.Scores.Happiness,
                        Neutral = f.Scores.Neutral,
                        Sadness = f.Scores.Sadness,
                        Surprise = f.Scores.Surprise
                    });
                }
            }
        }
    }
}