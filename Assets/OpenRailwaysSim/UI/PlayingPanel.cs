using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム中に表示される画面
/// </summary>
public class PlayingPanel : GamePanel
{

    public Toggle guideToggle;
    public Button removeButton;
    public Button placeBFButton;
    public Button placeBodyButton;
    public Button placeCouplerButton;
    public Button placePermanentCouplerButton;
    public Button setMotorsButton;
    public Button runButton;

    public void GuideButton()
    {
        Main.showGuide = guideToggle.isOn;
        Main.INSTANCE.reloadGrid();
    }

    public void ConstTrackButton()
    {
        Main.mode = Main.mode == Main.MODE_CONSTRUCT_TRACK ? 0 : Main.MODE_CONSTRUCT_TRACK;
    }

    public void PlaceAxleButton()
    {
        Main.mode = Main.mode == Main.MODE_PLACE_AXLE ? 0 : Main.MODE_PLACE_AXLE;
    }

    public void PlaceMapPinButton()
    {
        Main.mode = Main.mode == Main.MODE_PLACE_MAPPIN ? 0 : Main.MODE_PLACE_MAPPIN;
    }

    public void PlaceStructureButton()
    {
        Main.mode = Main.mode == Main.MODE_PLACE_STRUCTURE ? 0 : Main.MODE_PLACE_STRUCTURE;
    }

    public void RemoveButton()
    {
        Main.INSTANCE.removeSelectingObjs();
    }

    public void PlaceBFButton()
    {
        var bf = new BogieFrame(Main.playingmap, Main.selectingObjs.Where(obj => obj is Axle).OfType<Axle>().ToList());
        bf.generate();
        Main.playingmap.addObject(bf);
    }

    public void PlaceBodyButton()
    {
        var body = new Body(Main.playingmap, Main.selectingObjs.Where(obj => obj is BogieFrame).OfType<BogieFrame>().ToList());
        body.generate();
        Main.playingmap.addObject(body);
    }

    public void PlaceCouplerButton()
    {
        foreach (var body in Main.selectingObjs.Where(obj => obj is Body).OfType<Body>().ToList())
        {
            var c = new Coupler(Main.playingmap);
            c.body = body;
            Main.editingCoupler = c;
            c.generate();

            Main.INSTANCE.couplerSettingPanel.show(true);
            Main.INSTANCE.couplerSettingPanel.transform.position = new Vector3(
                Mathf.Clamp(Input.mousePosition.x, 0,
                    Screen.width - ((RectTransform)Main.INSTANCE.couplerSettingPanel.transform).rect
                    .width),
                Mathf.Clamp(Input.mousePosition.y,
                    ((RectTransform)Main.INSTANCE.couplerSettingPanel.transform).rect.height,
                    Screen.height));
            break;
        }
    }

    public void PlacePermanentCouplerButton()
    {
        var bodies = Main.selectingObjs.Where(obj => obj is Body).OfType<Body>().ToList();
        if (bodies.Count >= 2)
        {
            var c = new PermanentCoupler(Main.playingmap, bodies[0], bodies[1]);
            c.generate();
            Main.playingmap.addObject(c);
        }
    }

    public void SetMotorsButton()
    {
        foreach (var obj in Main.selectingObjs)
            if (obj is Body)
                ((Body)obj).motors = Main.selectingObjs.Where(o => o is Axle).OfType<Axle>().ToList();
    }

    public void RunButton()
    {
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is Body)
            {
                Main.INSTANCE.runPanel.body = (Body)obj;
                Main.INSTANCE.runPanel.show(true);
                break;
            }
        }
    }

    public void a()
    {
        removeButton.interactable = Main.selectingObjs.Any();
        placeBFButton.interactable = Main.selectingObjs.Any(obj => obj is Axle);
        placeBodyButton.interactable = Main.selectingObjs.Any(obj => obj is BogieFrame);
        placeCouplerButton.interactable = Main.selectingObjs.Any(obj => obj is Body);
        int a = 0;
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is Body)
                a++;
            if (a >= 2)
                break;
        }
        placePermanentCouplerButton.interactable = a >= 2;
        setMotorsButton.interactable = Main.selectingObjs.Any(obj => obj is Body);
        runButton.interactable = Main.selectingObjs.Any(obj => obj is Body);
    }

    public void b()
    {
        guideToggle.isOn = Main.showGuide;
    }
}
