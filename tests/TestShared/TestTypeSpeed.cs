namespace Test;

public partial class Test
{
    
    [CommandMethod(nameof(Test_TypeSpeed))]
    public void Test_TypeSpeed()
    {
        var line = new Line();
        var line1 = line as Entity;
        Tools.TestTimes(100000, "is 匹配：", () => {
            var t = line1 is Line;
        });
        Tools.TestTimes(100000, "name 匹配：", () => {
            // var t = line.GetType().Name;
            var tt = line1.GetType().Name == nameof(Line);
        });
        Tools.TestTimes(100000, "dxfname 匹配：", () => {
            // var t = line.GetType().Name;
            var tt = line1.GetRXClass().DxfName == nameof(Line);
        });
    }

    [CommandMethod(nameof(Test_sleeptrans))]
    public static void Test_sleeptrans()
    {
        using var tr = new DBTrans();
        for (int i = 0; i < 100; i++)
        {
            var cir = CircleEx.CreateCircle(new Point3d(i, i, 0), 0.5);
            if (cir is null)
            {
                return;
            }
            cir.ColorIndex = i;
            tr.CurrentSpace.AddEntity(cir);
            tr.Editor?.Redraw(cir);
            System.Threading.Thread.Sleep(10);
        }
    }
}