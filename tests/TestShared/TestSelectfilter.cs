namespace Test;

public class Testselectfilter
{
    [CommandMethod(nameof(Test_Filter))]
    public void Test_Filter()
    {
        var p = new Point3d(10, 10, 0);
        var f = OpFilter.Build(
        e => !(e.Dxf(0) == "line" & e.Dxf(8) == "0")
                | e.Dxf(0) != "circle" & e.Dxf(8) == "2" & e.Dxf(10) >= p);


        var f2 = OpFilter.Build(
        e => e.Or(
        !e.And(e.Dxf(0) == "line", e.Dxf(8) == "0"),
        e.And(e.Dxf(0) != "circle", e.Dxf(8) == "2",
        e.Dxf(10) <= new Point3d(10, 10, 0))));

        SelectionFilter f3 = f;
        SelectionFilter f4 = f2;

        Env.Editor.WriteMessage("");
    }

    [CommandMethod(nameof(Test_Selectanpoint))]
    public void Test_Selectanpoint()
    {
        var sel2 = Env.Editor.SelectAtPoint(new Point3d(0, 0, 0));
        Env.Editor.WriteMessage("");
    }
}

public class TestSelectObjectType
{
    [CommandMethod(nameof(Test_Select_type))]
    public void Test_Select_type()
    {
        var sel = Env.Editor.SSGet();
        if (sel.Status != PromptStatus.OK) return;
        var ids = sel.Value.GetObjectIds<Dimension>();
        foreach (var item in ids)
        {
            item.Print();
        }

        var dxfName = RXObject.GetClass(typeof(Dimension)).DxfName;
        dxfName.Print();
        var idss = sel.Value.GetObjectIds();
        foreach (var item in idss)
        {
            item.Print();
            item.ObjectClass.DxfName.Print();
        }
       
    }

}