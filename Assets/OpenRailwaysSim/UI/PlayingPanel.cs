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
    public Button settingButton;
    public Button placeBFButton;
    public Button placeBodyButton;
    public Button placeCouplerButton;
    public Button placePermanentCouplerButton;
    public Button setMotorsButton;
    public Button frontCabButton;
    public Button backCabButton;

    public void changedGuide()
    {
        Main.INSTANCE.reloadGrid();
    }

    public void ConstTrackButton()
    {
        Main.mode = Main.mode == Main.ModeEnum.CONSTRUCT_TRACK ? Main.ModeEnum.NONE : Main.ModeEnum.CONSTRUCT_TRACK;
    }

    public void PlaceAxleButton()
    {
        Main.mode = Main.mode == Main.ModeEnum.PLACE_AXLE ? Main.ModeEnum.NONE : Main.ModeEnum.PLACE_AXLE;
    }

    public void PlaceMapPinButton()
    {
        Main.mode = Main.mode == Main.ModeEnum.PLACE_MAPPIN ? Main.ModeEnum.NONE : Main.ModeEnum.PLACE_MAPPIN;
    }

    public void PlaceStructureButton()
    {
        Main.mode = Main.mode == Main.ModeEnum.PLACE_STRUCTURE ? Main.ModeEnum.NONE : Main.ModeEnum.PLACE_STRUCTURE;
    }

    public void RemoveButton()
    {
        Main.INSTANCE.removeSelectingObjs();
    }

    public void EditButton()
    {
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is Shape)
            {
                Main.editingTrack = (Shape)obj;

                Main.INSTANCE.shapeSettingPanel.isNew = false;
                Main.INSTANCE.shapeSettingPanel.show(true);
                Main.INSTANCE.shapeSettingPanel.transform.position = new Vector3(
                    Mathf.Clamp(Input.mousePosition.x, 0,
                        Screen.width - ((RectTransform)Main.INSTANCE.shapeSettingPanel.transform).rect
                        .width),
                    Mathf.Clamp(Input.mousePosition.y,
                        ((RectTransform)Main.INSTANCE.shapeSettingPanel.transform).rect.height,
                        Screen.height));
                break;
            }
            else if (obj is Body)
            {
                Main.editingBody = (Body)obj;

                Main.INSTANCE.bodySettingPanel.isNew = false;
                Main.INSTANCE.bodySettingPanel.show(true);
                Main.INSTANCE.bodySettingPanel.transform.position = new Vector3(
                    Mathf.Clamp(Input.mousePosition.x, 0,
                        Screen.width - ((RectTransform)Main.INSTANCE.bodySettingPanel.transform).rect
                        .width),
                    Mathf.Clamp(Input.mousePosition.y,
                        ((RectTransform)Main.INSTANCE.bodySettingPanel.transform).rect.height,
                        Screen.height));
                break;
            }
            else if (obj is Coupler)
            {
                Main.editingCoupler = (Coupler)obj;

                Main.INSTANCE.couplerSettingPanel.isNew = false;
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
            else if (obj is MapPin)
            {
                Main.editingMapPin = (MapPin)obj;

                Main.INSTANCE.mapPinSettingPanel.isNew = false;
                Main.INSTANCE.mapPinSettingPanel.show(true);
                Main.INSTANCE.mapPinSettingPanel.transform.position = new Vector3(
                    Mathf.Clamp(Input.mousePosition.x, 0,
                        Screen.width - ((RectTransform)Main.INSTANCE.mapPinSettingPanel.transform).rect
                        .width),
                    Mathf.Clamp(Input.mousePosition.y,
                        ((RectTransform)Main.INSTANCE.mapPinSettingPanel.transform).rect.height,
                        Screen.height));
                break;
            }
            else if (obj is Structure)
            {
                Main.editingStructure = (Structure)obj;

                Main.INSTANCE.structureSettingPanel.isNew = false;
                Main.INSTANCE.structureSettingPanel.show(true);
                Main.INSTANCE.structureSettingPanel.transform.position = new Vector3(
                    Mathf.Clamp(Input.mousePosition.x, 0,
                        Screen.width - ((RectTransform)Main.INSTANCE.structureSettingPanel.transform).rect
                        .width),
                    Mathf.Clamp(Input.mousePosition.y,
                        ((RectTransform)Main.INSTANCE.structureSettingPanel.transform).rect.height,
                        Screen.height));
                break;
            }
        }
    }

    public void PlaceBogieFrameButton()
    {
        var bf = new BogieFrame(Main.playingmap, Main.selectingObjs.Where(obj => obj is Axle).OfType<Axle>().ToList());
        bf.generate();
        Main.playingmap.addObject(bf);
    }

    public void PlaceBodyButton()
    {
        var b = new Body(Main.playingmap, Main.selectingObjs.Where(obj => obj is BogieFrame).OfType<BogieFrame>().ToList());
        Main.editingBody = b;
        b.generate();

        Main.INSTANCE.bodySettingPanel.isNew = true;
        Main.INSTANCE.bodySettingPanel.show(true);
        Main.INSTANCE.bodySettingPanel.transform.position = new Vector3(
            Mathf.Clamp(Input.mousePosition.x, 0,
                Screen.width - ((RectTransform)Main.INSTANCE.bodySettingPanel.transform).rect
                .width),
            Mathf.Clamp(Input.mousePosition.y,
                ((RectTransform)Main.INSTANCE.bodySettingPanel.transform).rect.height,
                Screen.height));
    }

    public void PlaceCouplerButton()
    {
        foreach (var body in Main.selectingObjs.Where(obj => obj is Body).OfType<Body>().ToList())
        {
            var c = new Coupler(Main.playingmap);
            c.body = body;
            Main.editingCoupler = c;
            c.generate();

            Main.INSTANCE.couplerSettingPanel.isNew = true;
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

    public void FrontCabButton()
    {
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is Body)
            {
                Main.INSTANCE.runPanel.cab = ((Body)obj).frontCab;
                Main.INSTANCE.runPanel.show(true);
                break;
            }
        }
    }

    public void BackCabButton()
    {
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is Body)
            {
                Main.INSTANCE.runPanel.cab = ((Body)obj).backCab;
                Main.INSTANCE.runPanel.show(true);
                break;
            }
        }
    }

    public void reloadInteractable()
    {
        removeButton.interactable = Main.selectingObjs.Any();
        settingButton.interactable = Main.selectingObjs.Any(obj => obj is Shape || obj is Body || obj is Coupler || obj is MapPin || obj is Structure);
        placeBFButton.interactable = Main.selectingObjs.Any(obj => obj is Axle);
        placeBodyButton.interactable = Main.selectingObjs.Any(obj => obj is BogieFrame);
        placeCouplerButton.interactable = Main.selectingObjs.Any(obj => obj is Body);
        var a = 0;
        foreach (var obj in Main.selectingObjs)
        {
            if (obj is Body)
                a++;
            if (a >= 2)
                break;
        }
        placePermanentCouplerButton.interactable = a >= 2;
        setMotorsButton.interactable = Main.selectingObjs.Any(obj => obj is Body);
        var b = (Body)Main.selectingObjs.FirstOrDefault(obj => obj is Body);
        if (b == null)
            frontCabButton.interactable = backCabButton.interactable = false;
        else
        {
            frontCabButton.interactable = b.frontCab != null;
            backCabButton.interactable = b.backCab != null;
        }
    }

    /// <summary>
    /// ガイドの表示を設定
    /// </summary>
    public void setGuide(bool isOn)
    {
        guideToggle.isOn = isOn;
    }
}
