using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveController : MonoBehaviour{

    [Header("Movement")]
    public float moveAccel = 2;
    public float maxSpeed = 4;

    [Header("Jump")]
    public float jumpAccel;

    private bool isJumping;
    private bool isOnGround;

    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPosX;

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPosY;

    [Header("Camera")]
    public CameraMoveController gameCamera;

    private Rigidbody2D rig;
    private Animator anim;
    private CharacterSoundController sound;

    // Start is called before the first frame update
    void Start(){
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();

        gameOverScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update(){
        // read input
        if (Input.GetMouseButtonDown(0)) {
            if (isOnGround) {
                isJumping = true;

                sound.PlayJump();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            //Debug.Log("Keluar");
            Application.Quit();
        }

        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.R)) {
            //Debug.Log("Reload");
            GameOver();
        }

        // change animation
        anim.SetBool("isOnGround", isOnGround);

        // calculate Score
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPosX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if(scoreIncrement > 0) {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPosX += distancePassed;
        }

        //Debug.Log(transform.position.y);

        // gameover
        if(transform.position.y < fallPosY) {
            GameOver();
        }

        if (score.isIncreaseSpeedRange) {
            maxSpeed += score.speedIncrease;
            score.isIncreaseSpeedRange = false;
            Debug.Log(maxSpeed);
        }
    }

    private void FixedUpdate() {
        // raycast ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit) {
            if (!isOnGround && rig.velocity.y <= 0) {
                isOnGround = true;
            }
        } else {
            isOnGround = false;
        }

        // calculate velocity vector
        Vector2 velocityVector = rig.velocity;

        if (isJumping) {
            velocityVector.y += jumpAccel;
            isJumping = false;
        }

        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);

        rig.velocity = velocityVector;
    }

    private void OnDrawGizmos() {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.white);
    }

    private void GameOver() {
        // set high score
        score.FinishScoring();

        // stop camera movement
        gameCamera.enabled = false;

        // show gameover
        gameOverScreen.SetActive(true);

        // disable this too
        this.enabled = false;
    }
}
