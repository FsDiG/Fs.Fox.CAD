namespace TestShared;

public class TestJson
{
    /*
     * 需要引入:
     * <ItemGroup>
     *     <Reference Include="System.Web" />
     *     <Reference Include="System.Web.Extensions" />
     * </ItemGroup>
     */
    [CommandMethod(nameof(JavaScriptSerializer))]
    public void JavaScriptSerializer()
    {
        List<int> RegisteredUsers = [];
        RegisteredUsers.Add(0);
        RegisteredUsers.Add(1);
        RegisteredUsers.Add(2);
        RegisteredUsers.Add(3);
        
        var serializedResult = System.Text.Json.JsonSerializer.Serialize(RegisteredUsers);
        var deserializedResult = System.Text.Json.JsonSerializer.Deserialize<List<int>>(serializedResult);

        
        
    }
}