using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    float speed = 4f;
    [SerializeField]
    Camera camera;
    Vector3 target = Vector3.zero;
    Animator animator = null;
    SpriteRenderer spriteRenderer = null;
    Vector2 direction = Vector2.right;

    private void Start() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = transform.position.z; // don't play with z axis
            target.y = transform.position.y; // stay on same horizontal strip
            direction = transform.position.x < target.x ? Vector2.right : Vector2.left;
            spriteRenderer.flipX = direction == Vector2.right;
        }
        if (target != Vector3.zero) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            bool isMoving = Mathf.Abs(transform.position.x - target.x) > 0.1f;
            animator.SetBool("is_moving", isMoving);
        }
        if (camera.transform.position.x - transform.position.x > 4) {
            camera.transform.position = new Vector3(transform.position.x + 4, 0, 0);
        } else if (transform.position.x - camera.transform.position.x > 4) {
            camera.transform.position = new Vector3(transform.position.x - 4, 0, 0);
        }
    }
}
