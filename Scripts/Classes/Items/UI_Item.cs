using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Item : MonoBehaviour
{
    private Property referencedProperty;

    public void setProperty(Property prop) {
        this.referencedProperty = prop;
    }

    public Property getProperty() {
        return referencedProperty;
    }
}
