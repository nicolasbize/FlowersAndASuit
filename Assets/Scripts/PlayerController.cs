using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float speed = 4f;
    [SerializeField] float distanceToInteraction = 3f;
    Vector3 target = Vector3.zero;
    Animator animator = null;
    SpriteRenderer spriteRenderer = null;
    Vector2 direction = Vector2.right;
    GameObject interactiveTarget = null;
    
    bool isMoving = false;

    private void Start() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = transform.position.z; // don't play with z axis
            target.y = transform.position.y; // stay on same horizontal strip
            Vector2 position2D = new Vector2(target.x, target.y);
            RaycastHit2D hit = Physics2D.Raycast(position2D, Vector2.zero);
            if (hit.collider != null) {
                var targetObject = hit.collider.gameObject;
                if (targetObject.GetComponent<Interactive>() != null) {
                    interactiveTarget = targetObject;
                    float distToLeft = Mathf.Abs(targetObject.transform.position.x - distanceToInteraction - transform.position.x);
                    float distToRight = Mathf.Abs(targetObject.transform.position.x + distanceToInteraction - transform.position.x);
                    if (distToLeft < distToRight) {
                        target.x = targetObject.transform.position.x - distanceToInteraction;
                    } else {
                        target.x = targetObject.transform.position.x + distanceToInteraction;
                    }
                    Debug.Log(hit.collider.gameObject.name);
                } else {
                    interactiveTarget = null;
                }
            } else {
                interactiveTarget = null;
            }
            direction = transform.position.x < target.x ? Vector2.right : Vector2.left;
            spriteRenderer.flipX = direction == Vector2.right;
        }
        if (target != Vector3.zero) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            isMoving = Mathf.Abs(transform.position.x - target.x) > 0.1f;
            animator.SetBool("is_moving", isMoving);
        }
        if (!isMoving && interactiveTarget != null) {
            direction = transform.position.x < interactiveTarget.transform.position.x ? Vector2.right : Vector2.left;
            spriteRenderer.flipX = direction == Vector2.right;
        }
    }
}
