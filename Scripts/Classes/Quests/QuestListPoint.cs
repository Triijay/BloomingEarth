using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuestListPoint : MonoBehaviour, IPointerClickHandler {

    public int QuestListIterator;

    public void OnPointerClick(PointerEventData eventData) {
        Globals.Game.currentWorld.QuestsComponent.loadQuest(QuestListIterator);
    }

}
