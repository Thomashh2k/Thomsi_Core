using Headless.Core.Payloads;
using Headless.DB;
using Headless.DB.DataObj;
using Headless.DB.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.PostgresTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Headless.Core.Managers
{
    public interface ICustomFormManager
    {
        public Task<CustomForm> GetCustomForm(Guid id);
        public Task<List<CustomForm>> GetCustomForms();
        public Task<List<string>> GetCustomFormData(Guid id);
        public Task<CustomForm> CreateCustomForm(CustomFormPL newCustomForm);
        public Task<CustomForm> UpdateCustomForm(Guid id, CustomFormPL newCustomForm);
        public void DeleteCustomForm(CustomForm newCustomForm);
        public Task<string> PostCustomFormData(string formName, string json);

    }
    public class CustomFormManager : ICustomFormManager
    {
        private HeadlessDbContext DbContext;
        private readonly IConfiguration Configuration;

        public CustomFormManager(HeadlessDbContext dbContext, IConfiguration configuration)
        {
            DbContext = dbContext;
            Configuration = configuration;
        }
        public async Task<CustomForm> CreateCustomForm(CustomFormPL data)
        {
            CustomForm customForm = new CustomForm
            {
                Id = Guid.NewGuid(),
                FormName = data.FormName,
                
            };

            CustomForm foundForm = DbContext.CustomForms.FirstOrDefault(cf => cf.FormName == customForm.FormName);
            if (foundForm != null)
                throw new Exception("FormName is used. Choosse a different name");

            string createTableSQLCommand = "CREATE TABLE cf_" + Regex.Replace(customForm.FormName, @"\s+", "") + "(" +
                "ID UUID PRIMARY KEY NOT NULL,";
            List<string> serializedInputs = new List<string>();

            for(var i = 0; i < data.Inputs.Length; i++)
            {
                //Seriliaze for CustomForm Table
                string serialized = JsonSerializer.Serialize(data.Inputs[i]);
                serializedInputs.Add(serialized);

                //Add in createTableSQLCommand for table creation
                createTableSQLCommand += data.Inputs[i].ToInput().ToSQLColumn();
                if (data.Inputs.Length - 1 != i)
                    createTableSQLCommand += ",";

            }
            createTableSQLCommand += ");";
            customForm.Inputs = serializedInputs.ToArray();

            DbContext.CustomForms.Add(customForm);
            try
            {
                await using var conn = new NpgsqlConnection(Configuration["DbConnectionString"]);
                await conn.OpenAsync();


                await using (var cmd = new NpgsqlCommand(createTableSQLCommand, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                //Not sure if we need to wait for the execution
                await conn.CloseAsync();

            }
            catch(Exception ex)
            {
                throw ex;
            }

            await DbContext.SaveChangesAsync();
            return customForm;
        }

        public async Task<CustomForm> UpdateCustomForm(Guid id, CustomFormPL updatedCustomForm)
        {
            CustomForm customForm = DbContext.CustomForms.Find(id);
            if (customForm == null)
                throw new Exception("CustomForm not found");


            customForm.FormName = (updatedCustomForm.FormName != "") ? updatedCustomForm.FormName : customForm.FormName;
            
            List<Input> inputsToBeDeleted = new List<Input>();
            List<Input> inputsToBeAdded = new List<Input>();
            List<Input> customFormInputsDeserialized = new List<Input>();

            foreach (var input in customForm.Inputs)
            {
                customFormInputsDeserialized.Add(Input.DeserializeInput(input));
            }

            for (int i = 0; i < updatedCustomForm.Inputs.Length; i++)
            {
                if (updatedCustomForm.Inputs[i].Delete == true)
                {
                    inputsToBeDeleted.Add(updatedCustomForm.Inputs[i].ToInput());
                }
                else if(updatedCustomForm.Inputs[i].New == true)
                {
                    inputsToBeAdded.Add(updatedCustomForm.Inputs[i].ToInput());

                }

            }

            string deleteColumnsSQLCommand = "ALTER TABLE cf_" + Regex.Replace(customForm.FormName, @"\s+", "");
            string addColumnsSQLCommand = "ALTER TABLE cf_" + Regex.Replace(customForm.FormName, @"\s+", "");

            for (int i = 0; i < inputsToBeDeleted.Count; i++)
            {
                deleteColumnsSQLCommand += " DROP COLUMN " + inputsToBeDeleted[i].InputName;
                if (inputsToBeDeleted.Count - 1 != i)
                    deleteColumnsSQLCommand += ",";
                // Strange errer when putting the line below directly in the .Find method
                var inputName = inputsToBeDeleted[i].InputName;
                var foundInput = customFormInputsDeserialized.Find(i => i.InputName == inputName);
                customFormInputsDeserialized.Remove(foundInput);
            }
            deleteColumnsSQLCommand += ';';
            for (int i = 0; i < inputsToBeAdded.Count; i++)
            {
                addColumnsSQLCommand += " ADD COLUMN " + inputsToBeAdded[i].InputName + ' ' + Input.ToPgsqlType(inputsToBeAdded[i].InputType);
                if (inputsToBeAdded.Count - 1 != i)
                    addColumnsSQLCommand += ",";

                customFormInputsDeserialized.Add(inputsToBeAdded[i]);
            }
            addColumnsSQLCommand += ';';


            try
            {
                await using var conn = new NpgsqlConnection(Configuration["DbConnectionString"]);
                await conn.OpenAsync();
                await using (var cmd = new NpgsqlCommand(deleteColumnsSQLCommand, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                await using (var cmd = new NpgsqlCommand(addColumnsSQLCommand, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await conn.CloseAsync();

            }
            catch(Exception ex)
            {
                throw ex;
            }

            List<string> serializedInputs = new List<string>();

            foreach (var input in customFormInputsDeserialized)
            {

                serializedInputs.Add(JsonSerializer.Serialize(input));
            }

            customForm.Inputs = serializedInputs.ToArray();
            DbContext.CustomForms.Update(customForm);

            await DbContext.SaveChangesAsync();

            return customForm;

        }

        public void DeleteCustomForm(CustomForm newCustomForm)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CustomForm>> GetCustomForms() => DbContext.CustomForms.ToList();
        public async Task<CustomForm> GetCustomForm(Guid id) => DbContext.CustomForms.Find(id);
        public async Task<List<string>> GetCustomFormData(Guid id)
        {
            CustomForm customForm = DbContext.CustomForms.Find(id);
            if (customForm == null)
                throw new Exception("CustomForm not found");

            string getCustomFormDataSQLCommand = "SELECT * FROM cf_" + Regex.Replace(customForm.FormName, @"\s+", "") + ";";
            List<string> customFormData = new List<string>();
            await using var conn = new NpgsqlConnection(Configuration["DbConnectionString"]);
            await conn.OpenAsync();

            try
            {
                await using (var cmd = new NpgsqlCommand(getCustomFormDataSQLCommand, conn))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {

                            for(ulong i = 0; i <= reader.Rows; i++)
                            {
                                string row = "{";
                                if(reader.GetDataTypeName(0) == "uuid")
                                {
                                    row += "\"ID\":\"" + reader.GetGuid(0).ToString() + "\",";
                                }
                                //-1 to ignore the ID column
                                //Refactor x+1 because its hardly to understand
                                for (int x = 0; x < reader.VisibleFieldCount-1; x++)
                                {
                                    var input = Input.DeserializeInput(customForm.Inputs[x]);
                                    var text = reader.GetDataTypeName(x + 1);
                                    if (reader.GetDataTypeName(x + 1) == "text")
                                    {
                                        row += '\"' + input.InputName + "\": \"" + reader.GetString(x+1).ToString();
                                    }
                                    else if (reader.GetDataTypeName(x+1) == "boolean")
                                    {
                                        row += '\"' + input.InputName + "\": \"" + reader.GetString(x+1).ToString();
                                    }
                                    else if (reader.GetDataTypeName(x+1) == "timestamp")
                                    {
                                        row += '\"' + input.InputName + "\": \"" + reader.GetString(x+1).ToString();
                                    }
                                    else if (reader.GetDataTypeName(x+1) == "time")
                                    {
                                        row += '\"' + input.InputName + "\": \"" + reader.GetString(x+1).ToString();
                                    }
                                    row += "\",";
                                }
                                var finalRow = row.Remove(row.Length - 1);
                                finalRow += '}';
                                customFormData.Add(finalRow);

                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            await conn.CloseAsync();
            //string JsonArray = "";
            //for (int i = 0; i < customFormData.Count; i++)
            //{
            //    JsonArray += customFormData[i] + ',';

            //}
            // var test1 = JsonConvert.DeserializeObject<object>(customFormData[);
            // var test2 = JsonSerializer.Serialize(customFormData);
            return customFormData;
        }
        public async Task<string> PostCustomFormData(string formName, string json)
        {
            dynamic dynJson = JsonConvert.DeserializeObject(json);
            string columns = "(";
            foreach(var item in dynJson)
            {
                columns += item.ToString() + ',';
            }
            columns += ")";
            string postDataSQLCommand = "INSERT INTO cf_" + formName + columns;
            return postDataSQLCommand;
        }
    }
}
