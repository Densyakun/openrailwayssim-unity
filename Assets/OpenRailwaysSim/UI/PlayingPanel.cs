using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//ゲーム中に表示される画面
public class PlayingPanel : GamePanel
{

    public Button removeButton;
    public Button placeBFButton;
    public Button placeBodyButton;

    public void ConstTrackButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_CONSTRUCT_TRACK ? 0 : Main.MODE_CONSTRUCT_TRACK;
    }

    public void PlaceAxleButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_PLACE_AXLE ? 0 : Main.MODE_PLACE_AXLE;
    }

    public void RemoveButton()
    {
        Main.removeSelectingObjs();
    }

    public void PlaceBFButton()
    {
        BogieFrame bf = new BogieFrame(Main.playingmap);
        bf.setAxles(Main.selectingObjs.Where(obj => obj is Axle).OfType<Axle>().ToList());
        bf.generate();
        Main.playingmap.addObject(bf);
    }

    public void PlaceBodyButton()
    {
        Body body = new Body(Main.playingmap);
        body.setBogieFrames(Main.selectingObjs.Where(obj => obj is BogieFrame).OfType<BogieFrame>().ToList());
        body.generate();
        Main.playingmap.addObject(body);
    }
}
