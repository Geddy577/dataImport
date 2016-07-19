using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseImporter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Button that starts the import of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImportData_Click(object sender, EventArgs e)
        {
            List<DataClass> data = new List<DataClass>();

            //Delete old folder if it exists
            deleteOldFolder();

            //Create new Database
            createDatabase();

            //Import data into list of custom class
            data = importDataToList();

            //Push data into new database
            insertListToDatabase(data);

            //Confirm correct entry
            countData(data);
        }

        /// <summary>
        /// Counts the amout of rows we got from the original database and compares it with the new one to see if we got all data
        /// </summary>
        /// <param name="data"></param>
        private void countData(List<DataClass> data)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(Defines.NEW_DATABASE_CONNECTION);
            int amountInList = 0;
            int amountInDatabase = 0;

            //Get count of data in list
            amountInList = data.Count;

            con.Open();

            //Find out how many rows we have in the database
            cmd.CommandText = "SELECT COUNT(*) "
                            + "FROM importedData";

            amountInDatabase = Convert.ToInt32(cmd.ExecuteScalar());

            con.Close();

            //Set up the final message
            messageBox.Text = "There are " + amountInDatabase.ToString() + " in the new database and " + amountInList.ToString() + " in the old database.";
            messageBox.Update();

            //Either give an all clear or an error messages depending on if the counts match
            if (amountInDatabase == amountInList)
            {
                messageBox.Text = "Transfer completed correctly.";
                messageBox.Update();
            }
            else
            {
                messageBox.Text = "Transfer completed incorrectly.  ERROR";
                messageBox.Update();
            }

            con.Close();
        }

        /// <summary>
        /// Creates the databse to insert the new data
        /// </summary>
        public void createDatabase()
        {
            //Create variables
            string cmd;
            SqlConnection con = new SqlConnection(Defines.NEW_DATABASE_CONNECTION);
            SqlCommand command = new SqlCommand(cmd, con);;

            //Commad for creating a database
            cmd = "CREATE DATABASE dataImport ON PRIMARY "
                + "(NAME = dataImport, "
                + "FILENAME = 'C:\\dataImport.mdf', "
                + "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%) ";


            try
            {
                con.Open();
                command.ExecuteNonQuery();
                messageBox.Text = "Database created successfully";
                messageBox.Update();
            }
            catch (Exception excep)
            {
                messageBox.Text = "Failed to create database reason: " + excep.ToString();
                messageBox.Update();
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        /// <summary>
        /// Creates the table in the database
        /// </summary>
        private void createTable()
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(Defines.NEW_DATABASE_CONNECTION);

            //Add all of the tables
            cmd.CommandText = "CREATE TABLE importedData "
                            + "( "
                            + "id int(10) "
                            + "); ";

            con.Open();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                messageBox.Text = "Table not created correctly.  Exception: " + ex.ToString();
                messageBox.Update();
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

        }

        /// <summary>
        /// Deletes the folder containing the database if it already created
        /// </summary>
        private void deleteOldFolder()
        {
            if (Directory.Exists(Defines.DATABASE_LOCATION))
            {
                //Call System.IO to delete location
                Directory.Delete(Defines.DATABASE_LOCATION);
            }
        }

        /// <summary>
        /// Gets the data from the other databases and puts it into a list that we can easily use
        /// </summary>
        /// <returns></returns>
        private List<DataClass> importDataToList()
        {
            List<DataClass> data = new List<DataClass>();
            DataClass row;
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(Defines.ORIG_DATABASE_CONNECTION);
            SqlDataReader reader;

            //Create command here, will use case statements to make sure empty data will be inserted with "NO DATA"
            //TODO create real sql when we have tables
            cmd.CommandText = "SELECT first_table.id "
                            + "FROM first_Table "
                            + "LEFT JOIN table2 ON first_table.id = second_table.fkid";

            //Open connection
            con.Open();

            //Set reader to sql query
            reader = cmd.ExecuteReader();

            //Iterate though records
            while(reader.Read())
            {
                //Create a new dataclass and add the correct items from the sql querry
                row = new DataClass(reader["id"].ToString());

                //Add the created row to the list
                data.Add(row);
            }

            //close connection
            con.Close();

            //Return data
            return data;
        }

        /// <summary>
        /// Insert the data into our new database
        /// </summary>
        /// <param name="data">List containing the data</param>
        private void insertListToDatabase(List<DataClass> data)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(Defines.NEW_DATABASE_CONNECTION);

            //Create table in new database
            createTable();

            con.Open();

            foreach (DataClass row in data)
            {
                //Clear command
                cmd = new SqlCommand();

                //Set all values with correct data type
                cmd.CommandText = "INSERT INTO importedData "
                                + "VALUES (id=?id)";
                cmd.Parameters.AddWithValue("?id", Convert.ToInt32(row.id));

                cmd.ExecuteNonQuery();
            }

            con.Close();


        }

    }
}
