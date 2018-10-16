using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//ゲーム中に表示される画面
public class PlayingPanel : GamePanel
{

    public Toggle guideToggle;
    public Button removeButton;
    public Button placeBFButton;
    public Button placeBodyButton;
    public Button placeCouplerButton;
    public Button placePermanentCouplerButton;
    public Button placeDirectControllerButton;
    public Button runButton;

    public void GuideButton()
    {
        Main.main.grid.SetActive(Main.main.showGuide = guideToggle.isOn);
        List<Track> objs = Main.playingmap.objs.Where(obj => obj is Track).OfType<Track>().ToList();
        foreach (var obj in objs)
        {
            obj.reloadTrackRenderer();
            obj.reloadRailRenderers();
        }
    }

    public void ConstTrackButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_CONSTRUCT_TRACK ? 0 : Main.MODE_CONSTRUCT_TRACK;
    }

    public void PlaceAxleButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_PLACE_AXLE ? 0 : Main.MODE_PLACE_AXLE;
    }

    public void PlaceMapPinButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_PLACE_MAPPIN ? 0 : Main.MODE_PLACE_MAPPIN;
    }

    public void RemoveButton()
    {
        Main.removeSelectingObjs();
    }

    public void PlaceBFButton()
    {
        BogieFrame bf = new BogieFrame(Main.playingmap, Main.selectingObjs.Where(obj => obj is Axle).OfType<Axle>().ToList());
        bf.generate();
        Main.playingmap.addObject(bf);
    }

    public void PlaceBodyButton()
    {
        Body body = new Body(Main.playingmap, Main.selectingObjs.Where(obj => obj is BogieFrame).OfType<BogieFrame>().ToList());
        body.generate();
        Main.playingmap.addObject(body);
    }

    public void PlaceCouplerButton()
    {
        var bodies = Main.selectingObjs.Where(obj => obj is Body).OfType<Body>().ToList();
        foreach (var body in bodies)
        {
            Coupler c = new Coupler(Main.playingmap);
            c.body = body;
            Main.editingCoupler = c;
            c.generate();

            GameCanvas.couplerSettingPanel.show(true);
            GameCanvas.couplerSettingPanel.transform.position = new Vector3(
                Mathf.Clamp(Input.mousePosition.x, 0,
                    Screen.width - ((RectTransform)GameCanvas.couplerSettingPanel.transform).rect
                    .width),
                Mathf.Clamp(Input.mousePosition.y,
                    ((RectTransform)GameCanvas.couplerSettingPanel.transform).rect.height,
                    Screen.height));
            break;
        }
    }

    public void PlacePermanentCouplerButton()
    {
        var bodies = Main.selectingObjs.Where(obj => obj is Body).OfType<Body>().ToList();
        if (bodies.Count >= 2)
        {
            PermanentCoupler c = new PermanentCoupler(Main.playingmap, bodies[0], bodies[1]);
            c.generate();
            Main.playingmap.addObject(c);
        }
    }

    public void PlaceDirectControllerButton()
    {
        Body body = null;
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is Body)
            {
                body = (Body)obj;
                break;
            }
        }
        DirectController dc = new DirectController(Main.playingmap, body, Main.selectingObjs.Where(obj => obj is Axle).OfType<Axle>().ToList());
        dc.generate();
        Main.playingmap.addObject(dc);
    }

    public void RunButton()
    {
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is DirectController)
            {
                GameCanvas.runPanel.show(true);
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
        placeDirectControllerButton.interactable = Main.selectingObjs.Any(obj => obj is Body);
        runButton.interactable = Main.selectingObjs.Any(obj => obj is DirectController);
    }

    public void b()
    {
        guideToggle.isOn = Main.main.showGuide;
    }
}
