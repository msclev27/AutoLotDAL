using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLotConnectedLayer
{
    class InventoryDAL
    {
        private SqlConnection sqlCn = null;

        public void OpenConnection(string connectionString)
        {
            sqlCn = new SqlConnection();
            sqlCn.ConnectionString = connectionString;
            sqlCn.Open();
        }

        public void Closeconnection()
        {
            sqlCn.Close();
        }

        public void InsertAuto(string id, string color, string make, string petName)
        {
            string sql = string.Format(
                "Insert Into Inventory" + 
                "(CarID, Make, Color, Petname) values" + 
                "('{0}', '{1}', '{2}', '{3}')", id, make, color, petName);

            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public class NewCar
        {
            public int CarID { get; set; }
            public String Color { get; set; }
            public String Make { get; set; }
            public String PetName { get; set; }
        }

        //public void InsertAuto(NewCar car)
        //{
        //    string sql = string.Format("Insert Into Inventory" +
        //        "(CarID, Make, Color, PetName) Values" +
        //        "('{0}', '{1}', '{2}', '{3}')", car.CarID, car.Make, car.Color, car.PetName);
        //    using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
        //    {
        //        cmd.ExecuteNonQuery();
        //    }
        //}

        public void InsertAuto(int id, string color, string make, string petName)
        {
            // Note the "placeholders" int he SQL query
            string sql = string.Format("insert Into Inventory" +
                "(CarID, Make, Color, PetName) Values" +
                "(@CarID, @Make, @Color, @PetName)");

            // This command will have internal parameters
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                // Fill Parameters collection
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@CarID";
                param.Value = id;
                param.SqlDbType = SqlDbType.Int;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Make";
                param.Value = make;
                param.SqlDbType = SqlDbType.Char;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Color";
                param.Value = color;
                param.SqlDbType = SqlDbType.Char;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@PetName";
                param.Value = petName;
                param.SqlDbType = SqlDbType.Char;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteCar(int id)
        {
            // Get ID of car to delete, then delete it.
            string sql = string.Format("Delete from Inventory where CarID = '{0};", id);
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(SqlException ex)
                {
                    Exception error = new Exception("Sorry! That car is on order!", ex);
                    throw error;
                }
            }
        }

        public void UpdateCarPetName(int id, string newPetName)
        {
            string sql = string.Format("Update Inventory Set PetName = '{0}; Where CarID = '{1}'", newPetName, id);

            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public List<NewCar> GetAllInventoryAsList()
        {
            //This will hold the records
            List<NewCar> inv = new List<NewCar>();

            // Prep command objects
            string sql = "Select * From Inventory";
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    inv.Add(new NewCar
                    {
                        CarID = (int)dr["CarId"],
                        Color = (string)dr["Color"],
                        Make = (string)dr["Make"],
                        PetName = (string)dr["PetName"]
                    });
                }
                dr.Close();
            }
            return inv;
        }

        public DataTable GetAllInventoryAsDataTable()
        {
            // This will hold the records
            DataTable inv = new DataTable();

            // Prep command object
            string sql = "Select * from Inventory";
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                //Fill the DataTable with data from the reader and clean up.
                inv.Load(dr);
                dr.Close();
            }
            return inv;
        }

        public string LookUpPetName(int carID)
        {
            string carPetName = string.Empty;

            // Establish name of stored proc.
            using (SqlCommand cmd = new SqlCommand("GetPetName", this.sqlCn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Input param.
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@carID";
                param.SqlDbType = SqlDbType.Int;
                param.Value = carID;

                // Default direction is Input, so this code is not needed
                param.Direction = ParameterDirection.Input();
                cmd.Parameters.Add(param);

                // Output Param
                param = new SqlParameter();
                param.ParameterName = "@petName";
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                // Execute the stored proc.
                cmd.EndExecuteNonQuery();

                // Return output param
                carPetName = (string)cmd.Parameters["@petName"].Value;
            }
            return carPetName;
        }
    }
}
