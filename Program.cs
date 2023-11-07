using Gtk;
using MySqlConnector;
MySqlConnection connection = new MySqlConnection();
Application.Init();
var window = new Window("TobySql");
window.Destroyed += (sender,e) => {
    Application.Quit();
};
var pohja = new VBox();
var hostEntry = new Entry();
hostEntry.PlaceholderText = "Hostname or IP";
pohja.Add(hostEntry);
var portEntry = new Entry();
portEntry.PlaceholderText = "Port";
pohja.Add(portEntry);
var userEntry = new Entry();
userEntry.PlaceholderText = "Username";
pohja.Add(userEntry);
var passEntry = new Entry();
passEntry.Visibility = false;
passEntry.PlaceholderText = "Password";
pohja.Add(passEntry);
var connectBtn = new Button();
connectBtn.Label = "Connect";
connectBtn.Clicked += async delegate{
    connection = new MySqlConnection($"Server={hostEntry.Text};Port={portEntry.Text};Uid={userEntry.Text};Pwd={passEntry.Text};AllowUserVariables=True;");
    try{
    connection.Open();
    if(connection.State == System.Data.ConnectionState.Open){
        MessageDialog messageDialog = new MessageDialog(window,DialogFlags.Modal,MessageType.Info,ButtonsType.Ok,"Connected!");
        messageDialog.Show();
        messageDialog.ButtonPressEvent += async (sender,e) => {
            messageDialog.Destroy();
            window.Hide();
            await OpenDatabaseForm(connection);
        };
    }
    }catch(Exception ex){
        MessageDialog messageDialog = new MessageDialog(window,DialogFlags.Modal,MessageType.Error,ButtonsType.None,ex.Message);
        messageDialog.Show();
    }

};
pohja.Add(connectBtn);
window.Add(pohja);
window.SetDefaultSize(300, 300);
window.ShowAll();
Application.Run();

async Task OpenDatabaseForm(MySqlConnection connection)
{
    window = new Window("TobySql");
    window.Destroyed += (sender,e) => {
        Application.Quit();
    };
    var vbox = new VBox();
    var connLabel = new Label($"Server version: {connection.ServerVersion}");
    vbox.Add(connLabel);
    window.Add(vbox);
    window.SetDefaultSize(300, 300);
    window.ShowAll();
}