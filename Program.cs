using Gtk;
using MySqlConnector;

class Program
{
    static MySqlConnection connection = new MySqlConnection();

    static void Main(string[] args)
    {
        Application.Init();

        var window = new Window("TobySql");
        window.Destroyed += (sender, e) => { Application.Quit(); };

        // Create a vertical box to hold the UI elements
        var vbox = new VBox();

        // Hostname or IP entry
        var hostEntry = CreateEntry("Hostname or IP");
        vbox.Add(hostEntry);

        // Port entry
        var portEntry = CreateEntry("Port");
        vbox.Add(portEntry);

        // Username entry
        var userEntry = CreateEntry("Username");
        vbox.Add(userEntry);

        // Password entry
        var passEntry = CreatePasswordEntry("Password");
        vbox.Add(passEntry);

        // Connect button
        var connectBtn = new Button();
        connectBtn.Label = "Connect";
        connectBtn.Clicked += async delegate
        {
            // Set up MySQL connection string using user input
            connection = new MySqlConnection($"Server={hostEntry.Text};Port={portEntry.Text};Uid={userEntry.Text};Pwd={passEntry.Text};AllowUserVariables=True;");

            try
            {
                await connection.OpenAsync();

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    // Show a message dialog on successful connection
                    ShowMessageDialog(window, "Connected!", MessageType.Info);

                    // Close the message dialog, hide the main window, and open the database form
                    window.Hide();
                    await OpenDatabaseForm(connection);
                }
            }
            catch (Exception ex)
            {
                // Show an error message dialog on connection failure
                ShowMessageDialog(window, ex.Message, MessageType.Error);
            }
        };
        vbox.Add(connectBtn);

        // Add the vertical box to the main window
        window.Add(vbox);

        // Set default window size and show all UI elements
        window.SetDefaultSize(300, 300);
        window.ShowAll();

        // Run the application
        Application.Run();
    }

    // Helper method to create a standard entry with a placeholder text
    static Entry CreateEntry(string placeholder)
    {
        var entry = new Entry();
        entry.PlaceholderText = placeholder;
        return entry;
    }

    // Helper method to create a password entry
    static Entry CreatePasswordEntry(string placeholder)
    {
        var entry = CreateEntry(placeholder);
        entry.Visibility = false; // Set entry visibility to false for password fields
        return entry;
    }

    // Helper method to show a message dialog
    static void ShowMessageDialog(Window parent, string message, MessageType messageType)
    {
        var messageDialog = new MessageDialog(parent, DialogFlags.Modal, messageType, ButtonsType.Ok, message);
        messageDialog.Show();
    }

    // Helper method to create a checkbox with a label
    static CheckButton CreateCheckbox(string label)
    {
        var checkbox = new CheckButton(label);
        return checkbox;
    }

    // Method to open the database form asynchronously
    static async Task OpenDatabaseForm(MySqlConnection connection)
    {
        var databaseWindow = new Window("TobySql");
        databaseWindow.Destroyed += (sender, e) => { Application.Quit(); };

        var vbox = new VBox();

        // Label for the database section
        var databaseLabel = new Label("Select Databases:");

        // Create checkboxes for databases from information_schema.schemata
        var databaseCheckboxes = new List<CheckButton>();
        using (var command = new MySqlCommand("SELECT schema_name FROM information_schema.schemata", connection))
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var dbName = reader.GetString(0);
                var checkbox = CreateCheckbox(dbName);
                databaseCheckboxes.Add(checkbox);
                vbox.Add(checkbox);
            }
        }

        // Text entry for SQL query
        var queryEntry = CreateEntry("Enter SQL Query");

        // Button to run the SQL query
        var runQueryBtn = new Button();
        runQueryBtn.Label = "Run Query";
        runQueryBtn.Clicked += async delegate
        {
            // Get selected databases
            var selectedDatabases = databaseCheckboxes
                .Where(checkbox => checkbox.Active)
                .Select(checkbox => checkbox.Label)
                .ToList();

            // Get the SQL query from the textbox
            var sqlQuery = queryEntry.Text;

            // Execute the SQL query on selected databases
            try
            {
                foreach (var dbName in selectedDatabases)
                {
                    using (var command = new MySqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@dbname", dbName);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Process the query results as needed
                        }
                    }
                }

                // Show a success message
                ShowMessageDialog(databaseWindow, "Query executed successfully!", MessageType.Info);
            }
            catch (Exception ex)
            {
                // Show an error message dialog on query execution failure
                ShowMessageDialog(databaseWindow, ex.Message, MessageType.Error);
            }
        };

        // Add database section, query entry, and run query button to the vbox
        vbox.Add(databaseLabel);
        vbox.Add(queryEntry);
        vbox.Add(runQueryBtn);

        databaseWindow.Add(vbox);
        databaseWindow.SetDefaultSize(400, 300);
        databaseWindow.ShowAll();
    }
}