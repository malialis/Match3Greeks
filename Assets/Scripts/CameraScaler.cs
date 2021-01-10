using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    private TaftBoard board;
    [Header("Camera Aspect Ratio settings")]
    public float cameraOffset;
    public float aspectRatio = 0.625f;
    public float padding = 2f;
    public float yOffset = 1f;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();
        if(board != null)
        {
            RepositionCamera(board.width -1, board.height -1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RepositionCamera(float x, float y)
    {
        Vector3 tempPosition = new Vector3(x / 2, y / 2 + yOffset, cameraOffset);
        transform.position = tempPosition;

        if(board.width >= board.height)
        {
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = board.height / 2 + padding;
        }
        
    }




}
