using Headless.Core.Payloads;
using Headless.DB;
using Headless.DB.DataObj;
using Headless.DB.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Headless.Core.Managers
{
    public interface ICustomFormManager
    {
        public Task<CustomForm> CreateCustomForm(CustomFormPL newCustomForm);
        public Task<CustomForm> UpdateCustomForm(Guid id, CustomFormPL newCustomForm);
        public void DeleteCustomForm(CustomForm newCustomForm);
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

            string createTableSQLCommand = "CREATE TABLE cf_" + customForm.FormName + "(" +
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

            await using var conn = new NpgsqlConnection(Configuration["DbConnectionString"]);
            await conn.OpenAsync();


            await using (var cmd = new NpgsqlCommand(createTableSQLCommand, conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            await DbContext.SaveChangesAsync();
            //Not sure if we need to wait for the execution
            await conn.CloseAsync();

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

            string deleteColumnsSQLCommand = "ALTER TABLE cf_" + customForm.FormName;
            string addColumnsSQLCommand = "ALTER TABLE cf_" + customForm.FormName;

            for (int i = 0; i < inputsToBeDeleted.Count; i++)
            {
                deleteColumnsSQLCommand += " DROP COLUMN " + inputsToBeDeleted[i].InputName;
                if (inputsToBeDeleted.Count - 1 != i)
                    deleteColumnsSQLCommand += ",";

                var foundInput = customFormInputsDeserialized.Find(i => i.InputName == inputsToBeDeleted[i].InputName);
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

            List<string> serializedInputs = new List<string>();

            foreach (var input in customFormInputsDeserialized)
            {

                serializedInputs.Add(JsonSerializer.Serialize(input));
            }

            customForm.Inputs = serializedInputs.ToArray();
            DbContext.CustomForms.Update(customForm);

            await DbContext.SaveChangesAsync();
            await conn.CloseAsync();

            return customForm;

        }

        public void DeleteCustomForm(CustomForm newCustomForm)
        {
            throw new NotImplementedException();
        }
    }
}
