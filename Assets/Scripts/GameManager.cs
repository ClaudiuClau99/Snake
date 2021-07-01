using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Snake
{

    public class GameManager : MonoBehaviour
    {
        public int maxHeight = 15;
        public int maxWidth = 17;

        public Color color1;
        public Color color2;
        public Color appleColor = Color.red;
        public Color playerColor = Color.black;

        public Transform cameraHolder;

        GameObject playerObj;
        GameObject appleObj;
        GameObject tailParent;
        Node playerNode;
        Node appleNode;
        Node prevPlayerNode;
        Sprite playerSprite;

        GameObject mapObject;
        SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();//nodurile libere de pe harta
        List<SpecialNode> tail = new List<SpecialNode>();

        bool up, right, left, down;

        int currentScore;
        int highScore;

        public bool isGameOver;
        public bool isFirstInput;
        public float moveRate = 0.5f;
        float timer;

        Direction targetDirection;
        Direction curDirection;

        public Text currentScoreText;
        public Text highScoreText;

        public enum Direction { up, right, left, down }

//Evenimente
        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;
        public UnityEvent onScore;


        void Start()
        {
            onStart.Invoke();
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            CreateApple();
            targetDirection = Direction.right;
            isGameOver = false;
            currentScore = 0;
            UpdateScore();
        }

        public void ClearReferences()
        {
            if (mapObject != null)
                Destroy(mapObject);
            if (playerObj != null)
                Destroy(playerObj);
            if (appleObj != null)
                Destroy(appleObj);
            foreach (var item in tail)
            {
                if (item.obj != null)
                    Destroy(item.obj);
            }
            tail.Clear();
            availableNodes.Clear();
            grid = null;
        }

        void CreateMap()
        {
            mapObject = new GameObject("Map");
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();

            grid = new Node[maxWidth, maxHeight];

            Texture2D txt = new Texture2D(maxWidth, maxHeight);
            for (int i = 0; i < maxWidth; i++)
            {
                for (int j = 0; j < maxHeight; j++)
                {

                    Vector3 tp = Vector3.zero;
                    tp.x = i;
                    tp.y = j;


                    Node n = new Node()
                    {

                        x = i,
                        y = j,
                        worldPosition = tp
                    };
                    grid[i, j] = n;

                    availableNodes.Add(n);

                    if ((i + j) % 2 == 0)
                    { txt.SetPixel(i, j, color1); }
                    else
                    { txt.SetPixel(i, j, color2); }


                }
                txt.filterMode = FilterMode.Point;

                txt.Apply();
                Rect rect = new Rect(0, 0, maxWidth, maxHeight);
                Sprite sprite = Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
                mapRenderer.sprite = sprite;

            }
        }

        void PlacePlayer()
        {
            playerObj = new GameObject("Player");
            SpriteRenderer playerRender = playerObj.AddComponent<SpriteRenderer>();
            playerSprite = CreateSprite(playerColor);
            playerRender.sprite = playerSprite;
            playerRender.sortingOrder = 1;
            playerNode = GetNode(3, 3);
            PlacePlayerObject(playerObj, playerNode.worldPosition);
            playerObj.transform.localScale = Vector3.one * 1.2f;

            tailParent = new GameObject("tailParent");

        }

        void PlaceCamera()
        {
            Node n = GetNode(maxWidth / 2, maxHeight / 2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * .5f;
            cameraHolder.position = p;
        }

        void CreateApple()
        {
            appleObj = new GameObject("Apple");
            SpriteRenderer appleRenderer = appleObj.AddComponent<SpriteRenderer>();
            appleRenderer.sprite = CreateSprite(appleColor);
            appleRenderer.sortingOrder = 1;
            RandomlyPlaceApple();
        }


        private void Update()
        {

            if (isGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    onStart.Invoke();
                }
                return;
            }

            GetInput();

            if (isFirstInput)
            {
                SetPlayerDirection();
                timer += Time.deltaTime;
                if (timer > moveRate)
                {
                    timer = 0;
                    curDirection = targetDirection;
                    MovePlayer();
                }
            }
            else
            {
                if (up || down || left || right)
                {
                    isFirstInput = true;
                    firstInput.Invoke();
                }
            }


        }





        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        void SetPlayerDirection()
        {
            if (up)
            {
                SetDirection(Direction.up);


            }
            else if (down)
            {
                SetDirection(Direction.down);

            }
            else if (left)
            {
                SetDirection(Direction.left);

            }
            else if (right)
            {
                SetDirection(Direction.right);

            }
        }

        void SetDirection(Direction d)
        {
            if (!IsOpposite(d))
            {
                targetDirection = d;
            }
        }

        void MovePlayer()
        {

            int x = 0;
            int y = 0;
            switch (curDirection)
            {
                case Direction.up:
                    y = 1;
                    break;
                case Direction.down:
                    y = -1;
                    break;
                case Direction.left:
                    x = -1;
                    break;
                case Direction.right:
                    x = 1;
                    break;
            }

            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);
            if (targetNode == null)
            {
                //Gane over
                onGameOver.Invoke();
            }
            else
            {
                if (IsTailNode(targetNode))
                {
                    //GameOver
                    onGameOver.Invoke();
                }
                else
                {
                    bool isScore = false;
                    if (targetNode == appleNode)
                    {
                        isScore = true;
                    }

                    Node previouseNode = playerNode;
                    availableNodes.Add(previouseNode);


                    if (isScore)
                    {
                        tail.Add(CreateTailNode(previouseNode.x, previouseNode.y));
                        availableNodes.Remove(previouseNode);
                    }

                    MoveTail();

                    PlacePlayerObject(playerObj, targetNode.worldPosition);

                    playerNode = targetNode;
                    availableNodes.Remove(playerNode);

                    if (isScore)
                    {
                        currentScore++;
                        if (currentScore > highScore)
                        {
                            highScore = currentScore;
                        }
                        onScore.Invoke();
                        if (availableNodes.Count > 0)
                        { RandomlyPlaceApple(); }
                        else
                        {//Win}
                        }
                    }
                }
            }
        }

        void MoveTail()
        {
            Node prevNode = null;
            for (int i = 0; i < tail.Count; i++)
            {
                SpecialNode p = tail[i];
                availableNodes.Add(p.node);
                if (i == 0)
                {
                    prevNode = p.node;
                    p.node = playerNode;
                }
                else
                {
                    Node prev = p.node;
                    p.node = prevNode;
                    prevNode = prev;
                }

                availableNodes.Remove(p.node);
                PlacePlayerObject(p.obj, p.node.worldPosition);

            }
        }
        
        // verifica daca un nod face parte din coada
        bool IsTailNode(Node n) 
        {
            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].node == n)
                {
                    return true;
                }
            }
            return false;
        }

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * .5f;
            obj.transform.position = pos;

        }

        void RandomlyPlaceApple()
        {
            int ran = Random.Range(0, availableNodes.Count);
            Node n = availableNodes[ran];
            PlacePlayerObject(appleObj, n.worldPosition);
            appleNode = n;
        }

        public void GameOver()
        {
            isGameOver = true;
            isFirstInput = false;
        }

        public void UpdateScore()
        {
            currentScoreText.text = currentScore.ToString();
            highScoreText.text = highScore.ToString();
        }

        bool IsOpposite(Direction d)
        {
            switch (d)
            {
                default:
                case Direction.up:
                    if (curDirection == Direction.down)
                        return true;
                    else
                        return false;
                case Direction.down:
                    if (curDirection == Direction.up)
                        return true;
                    else
                        return false;
                case Direction.left:
                    if (curDirection == Direction.right)
                        return true;
                    else
                        return false;
                case Direction.right:
                    if (curDirection == Direction.left)
                        return true;
                    else
                        return false;
            }
        }

        Node GetNode(int x, int y)  //returneaza pozitia unui nod de pe harta
        {
            if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1)
                return null;
            return grid[x, y];
        }

        SpecialNode CreateTailNode(int x, int y)
        {
            SpecialNode s = new SpecialNode();
            s.node = GetNode(x, y);
            s.obj = new GameObject();
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.position = s.node.worldPosition;
            s.obj.transform.localScale = Vector3.one * .95f;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = playerSprite;
            r.sortingOrder = 1;
            return s;
        }

        Sprite CreateSprite(Color targetColor)
        {
            Texture2D txt = new Texture2D(1, 1);
            txt.SetPixel(0, 0, targetColor);
            txt.Apply();
            txt.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(txt, rect, Vector2.one * .5f, 1, 0, SpriteMeshType.FullRect);
        }



    }
}
