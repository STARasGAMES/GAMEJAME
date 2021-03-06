﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController2D : MonoBehaviour {

	[SerializeField] PointerController _pointer;
    [SerializeField] Transform _groundDetector;
    [Header("Animation")]
    [SerializeField] Animator _animator;
    [SerializeField] float _speedMult = 0.1f;
    [Header("Moving")]
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] float _stopRange = 0.2f;
	[SerializeField] float _visibilityRange = 5;
    [SerializeField] float _moveForce = 10;
    [SerializeField] float _fallForce = 100;
    [Header("Jump")]
    [SerializeField] float _jumpRange = 3;
    [SerializeField] float _jumpNeededSpeed = 5;
    [SerializeField] float _jumpForce = 1000;
    [SerializeField] float _jumpDuration = 1;
    [SerializeField] float _jumpUpHeight = 1;
    [SerializeField] JumpType _curJumpType;
    [SerializeField] AnimationCurve _jumpUpCurve;
    [SerializeField] AnimationCurve _jumpForwardCurve;
    [SerializeField] AnimationCurve _jumpDownCurve;

    public enum JumpType
    {
        up,
        forward,
        down
    }

    Rigidbody2D _rigidbody;
    Vector2 force;

    bool _isJumping;
    Vector2 _jumpStartPos;
    Vector2 _jumpTargetPos;
    float _jumpStartTime;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

		//_climbObjects = GameObject.FindGameObjectsWithTag("Climb");
		//Debug.Log ("Climb objects");
		//Debug.Log (_climbObjects);
    }

    // Use this for initialization
    void Start () {
		
	}
	
    public enum State
    {
        noInput,
        input

    }

	private bool CanSeePointer() 
	{
		return (_pointer.transform.position - transform.position).magnitude < _visibilityRange;
	}

    private void Update()
    {
        // if (cat is on any of _climbObjects) {
        //     disable gravity
        //     allow cat to move on wall in direction of pointer if there's climb area 
        //     
        // } else {
        //     enable physics
        // }
        if (_isJumping)
        {
            Debug.DrawRay(_jumpStartPos, Vector3.up, Color.blue);
            Debug.DrawRay(_jumpTargetPos, Vector3.up, Color.blue);

            float curVal = (Time.time - _jumpStartTime) / _jumpDuration;
            float x = Mathf.Lerp(_jumpStartPos.x, _jumpTargetPos.x, curVal);
            float y = _jumpStartPos.y + _jumpUpCurve.Evaluate(curVal) * (_jumpTargetPos.y - _jumpStartPos.y);
            switch (_curJumpType)
            {
                case JumpType.up:
                    x = Mathf.Lerp(_jumpStartPos.x, _jumpTargetPos.x, curVal);
                    y = _jumpStartPos.y + _jumpUpCurve.Evaluate(curVal) * (_jumpTargetPos.y - _jumpStartPos.y);
                    break;
                case JumpType.forward:
                    x = Mathf.Lerp(_jumpStartPos.x, _jumpTargetPos.x, curVal);
                    y = _jumpStartPos.y + _jumpForwardCurve.Evaluate(curVal) * (Mathf.Abs(_jumpTargetPos.x - _jumpStartPos.x));
                    break;
                case JumpType.down:
                    Debug.LogError("WTF");
                    break;
                default:
                    break;
            }
            transform.position = new Vector3(x, y, transform.position.z);
            if (curVal >= 1)
            {
                _isJumping = false;
                GetComponent<Collider2D>().isTrigger = false;
                _rigidbody.isKinematic = false;
                _rigidbody.velocity = new Vector2(
                    (_jumpTargetPos.x - _jumpStartPos.x)*3,
                    -3
                    );
                    
            }
            return;
        }

        force = Vector3.zero;
        if (!IsGrounded())
        {
            _rigidbody.AddForce(Vector2.down*_fallForce*Time.deltaTime);
            //Debug.Log("not Grounded");
            return;
        }

        if (!CanSeePointer())
        {
            //Debug.Log("dont see pointer");
            return;
        }
        Vector2 targetPos = Vector3.zero;
        Collider2D targetCollider = null;
        //if (_pointer.isCastOnSomething)
        //{
        //    if (_pointer.castedCollider.CompareTag("Obstacle"))
        //    {
        //        Debug.Log("Pointer on Obstacle");
        //        _pointer.SetActive(false);
        //        return;
        //    }
        //    targetPos = _pointer.transform.position;
        //}
        //else
        //{
            Vector2 pos = new Vector2(_pointer.transform.position.x, _pointer.transform.position.y);
            Debug.DrawRay(pos + Vector2.up, Vector2.down, Color.white);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 100);
            if (hit.transform == null)
            {
                //Debug.Log("cant find collider");
                return;
            }
            if (hit.collider.CompareTag("Obstacle"))
            {
                //Debug.Log("Pointer on Obstacle");
                _pointer.SetActive(false);
                return;
            }
        if (hit.collider.CompareTag("Walkable"))
        {
            targetPos = hit.point;
            targetCollider = hit.collider;
        }
        //else
        //    Debug.Log("error");

        _pointer.SetActive(true);
        Vector2 some = targetPos - new Vector2(transform.position.x, transform.position.y);
        Vector2 dir = new Vector2(some.x, some.y);
        if ( Mathf.Abs(dir.x) < _stopRange)
        {
            //Debug.Log("in stop range");
            return;
        }
        if (targetCollider != _groundCollider && ((targetPos.y - transform.position.y) > _jumpUpHeight || _edgeCollider != null))
        {
            //Debug.Log("Need to jump");
            if (dir.magnitude < _jumpRange)
            {
                // JUMP UP
                if ((targetPos.y - transform.position.y) > _jumpUpHeight)
                    if (Mathf.Abs(_rigidbody.velocity.x) > _jumpNeededSpeed)
                    {
                        //Debug.Log("JUMP UP!");
                        _curJumpType = JumpType.up;
                        //_rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                        _isJumping = true;
                        _jumpStartPos = new Vector2(transform.position.x, transform.position.y);
                        _jumpTargetPos = targetPos;
                        _jumpStartTime = Time.time;
                        GetComponent<Collider2D>().isTrigger = true;
                        _rigidbody.isKinematic = true;
                        return;
                    }
                    else
                    {
                        //Debug.Log("not enough speed");
                    }
                // JUMP FORWARD
                if (true)//(targetPos.y - transform.position.y) < _jumpUpHeight && (targetPos.y - transform.position.y) > -_jumpUpHeight)
                {
                    if (Mathf.Abs(_rigidbody.velocity.x) > _jumpNeededSpeed)
                    {
                        //Debug.Log("JUMP FORWARD!");
                        _curJumpType = JumpType.forward;
                        //_rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                        _isJumping = true;
                        _jumpStartPos = new Vector2(transform.position.x, transform.position.y);
                        _jumpTargetPos = targetPos;
                        _jumpStartTime = Time.time;
                        GetComponent<Collider2D>().isTrigger = true;
                        _rigidbody.isKinematic = true;
                        return;
                    }
                    else
                    {
                        //Debug.Log("not enough speed");
                    }
                }
            }
            // JUMP DOWN
        }
        force = new Vector2(dir.x, 0).normalized * _moveForce * Time.fixedDeltaTime;
        _rigidbody.AddForce(force);
		Debug.DrawRay (transform.position, force, Color.green);
        Debug.DrawRay(targetPos, Vector3.up, Color.yellow);

        // ANIMATION
        
    }

    private void LateUpdate()
    {
        _animator.SetBool("Jump", _isJumping);
        if (_isJumping)
            return;
        if (Mathf.Sign(_rigidbody.velocity.x) == Mathf.Sign(force.x))
        {
            
            _renderer.flipX = force.x < 0 && Mathf.Abs(force.x) > 1;
            _animator.SetFloat("Speed", _rigidbody.velocity.magnitude * Time.deltaTime * _speedMult);
        }
        else
            _animator.SetFloat("Speed", -_rigidbody.velocity.magnitude * Time.deltaTime * _speedMult* 10);
    }

    Collider2D _groundCollider;
    Collider2D _edgeCollider;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Edge"))
        {
            _edgeCollider = collider;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Edge"))
        {
            if (_edgeCollider!=null && collider != _edgeCollider)
                Debug.LogError("WTF");
            _edgeCollider = collider;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Edge"))
        {
            if (collider != _edgeCollider)
                Debug.LogError("WTF");
            _edgeCollider = null;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Walkable"))
        {
            _groundCollider = collision.collider;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Walkable") && collision.collider)
        {
            _groundCollider = null;
        }
    }

    private bool IsGrounded()
    {
        if (_groundCollider)
            return true;
        Vector2 pos = new Vector2(_groundDetector.position.x, _groundDetector.position.y);
        var hit = Physics2D.Raycast(pos, Vector2.down, 0.2f);
        Debug.DrawRay(transform.position, Vector3.down, Color.green);
        if (hit.transform != null && (hit.point - pos).magnitude < _stopRange && hit.transform.CompareTag("Walkable"))
            return true;
        return false;
    }

    private bool CanMoveInDir(Vector2 dir)
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        var hit = Physics2D.Raycast(pos, dir, 1000);
        Debug.DrawRay(transform.position, dir, Color.green);
        if (hit.transform != null && (hit.point - pos).magnitude < _stopRange && hit.transform.CompareTag("Walkable"))
            return true;
        return false;
    }
}
