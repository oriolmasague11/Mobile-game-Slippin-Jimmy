using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PlayerControler : MonoBehaviour
{
    public float speed_snow;
    public float speed_ice;
    private float speed; 

    private Vector2 puntoMov;
    public Vector2 offset;
    private bool moviendo;
    private float right;
    private float top;
    public LayerMask gel;
    public LayerMask nuvol; 
    public LayerMask roca;
    public LayerMask plataforma;
    public float radioCirculo;
    private Vector2 input;
    public float desplazamiento;

    private Animator _animator;
    private bool skiing;

    public ParticleSystem impact; 
    public GameObject dust;

    private bool alt;

    public bool dust_act;
    public bool impacte_act;

    private bool _isPaused;
    public GameObject menu;

    public GameObject startPoint; 
    public GameObject endPoint;
    private Vector3 nextPoint; 
    private bool _isInitialising;
    private bool _isEnding;
    private float timer;

    public LevelLoader level;

    public ParticleSystem ice_trail;

    private bool pausa_final;
    public float tempsEsperaFinal;

    private Vector2 puntoImpacto;
    private bool impactat; 

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        puntoMov = transform.position;

        transform.position = new Vector3(startPoint.transform.position.x, 
            startPoint.transform.position.y - 2f*desplazamiento, 
            startPoint.transform.position.z);

        moviendo = false; 
        right = top = 0;
        skiing = false;

        speed = speed_snow;
        alt = false; 
        _isPaused = false;
        timer = 0; 

        _isInitialising = true;
        _isEnding = false; 
        nextPoint = new Vector3(startPoint.transform.position.x,
            startPoint.transform.position.y - desplazamiento,
            startPoint.transform.position.z);

        ice_trail.Stop();
        pausa_final = false; 

        impactat = false;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                pausa_final = false;
                level.loadNextLevel();
            }
        }
    }

    private void executarImpacte(Vector2 puntoEvaluar)
    {
        Vector2 _lookDir = new Vector2(transform.position.x, transform.position.y) - puntoEvaluar;

        Vector3 puntoExe = new Vector3(puntoEvaluar.x, puntoEvaluar.y, 0);
        if(puntoEvaluar.x == transform.position.x) puntoExe.y = (puntoEvaluar.y + transform.position.y) / 2f;
        else if (puntoEvaluar.y == transform.position.y) puntoExe.x = (puntoEvaluar.x + transform.position.x) / 2f;

        ParticleSystem p; 
        //cas especial per si esta de pujada. 
        if (puntoEvaluar.y > transform.position.y) p = Instantiate(impact, new Vector3(puntoEvaluar.x, puntoEvaluar.y, 0), Quaternion.LookRotation(_lookDir));
        else p = Instantiate(impact, puntoExe, Quaternion.LookRotation(_lookDir));
        Destroy(p, 1f);
    }

    private void executarDust()
    {
        GameObject d = Instantiate(dust, transform.position, Quaternion.identity);
        Destroy(d, 0.5f);
    }

    private void movimientoInicial()
    {
        moviendo = true;
        _animator.SetBool("isMoving", moviendo);
        _animator.SetFloat("moveX", 0f);
        _animator.SetFloat("moveY", 1f);
        transform.position = Vector2.MoveTowards(transform.position, nextPoint, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, nextPoint) == 0)
        {
            alt = !alt;
            if (nextPoint == startPoint.transform.position) { _isInitialising = false; moviendo = false; }
            else
            {
                nextPoint = startPoint.transform.position;
                _animator.SetBool("alternate", alt);
            }
        }
    }

    private void movimientoFinal()
    {
        moviendo = true;
        _animator.SetBool("isMoving", moviendo);
        _animator.SetFloat("moveX", 0f);
        _animator.SetFloat("moveY", 1f);
        transform.position = Vector2.MoveTowards(transform.position, nextPoint, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, nextPoint) == 0)
        {
            alt = !alt;
            if (nextPoint == new Vector3(endPoint.transform.position.x, endPoint.transform.position.y + 2 * desplazamiento, 0))
            {
                _isEnding = false;
                moviendo = false;
            }
            else
            {
                nextPoint = new Vector3(endPoint.transform.position.x, endPoint.transform.position.y + 2 * desplazamiento, 0);
                _animator.SetBool("alternate", alt);
            }
        }
    }


    private void iniciarNuevoMovimiento()
    {
        Vector2 puntoEvaluar = new Vector2(transform.position.x, transform.position.y) + offset + input * desplazamiento;

        if ((!Physics2D.OverlapCircle(puntoEvaluar, radioCirculo, roca) && !Physics2D.OverlapCircle(puntoEvaluar, radioCirculo, nuvol))
            || Physics2D.OverlapCircle(puntoEvaluar, radioCirculo, plataforma) && !Physics2D.OverlapCircle(puntoEvaluar, radioCirculo, roca))
        {
            moviendo = true;
            puntoMov += input * desplazamiento;
            alt = !alt;
            _animator.SetBool("alternate", alt);
            if (Physics2D.OverlapCircle(puntoEvaluar, radioCirculo, gel) && !Physics2D.OverlapCircle(puntoEvaluar, radioCirculo, nuvol))  //gel
            {
                if (!skiing && dust_act) executarDust();
                skiing = true;
                ice_trail.transform.rotation = Quaternion.LookRotation(new Vector2(-input.x, -input.y));
                ice_trail.Play();
                speed = speed_ice;
                //cas pel gel 
                while((!Physics2D.OverlapCircle(puntoMov, radioCirculo, roca) && !Physics2D.OverlapCircle(puntoMov, radioCirculo, nuvol))
                        || Physics2D.OverlapCircle(puntoMov, radioCirculo, plataforma) && !Physics2D.OverlapCircle(puntoMov, radioCirculo, roca))
                {
                    puntoMov += input * desplazamiento;
                }
                if (Physics2D.OverlapCircle(puntoMov, radioCirculo, roca)) { puntoImpacto = puntoMov; impactat = true; }
                puntoMov -= input * desplazamiento;
            }
        }
        else
        { 
            moviendo = false; 
            skiing = false;
            ice_trail.Stop();
            speed = speed_snow;
        }
    }


    private void FixedUpdate()
    {
        if (_isInitialising) movimientoInicial();
        else if (_isEnding && !pausa_final) movimientoFinal(); 
        else
        {
            if (moviendo)
            {
                transform.position = Vector2.MoveTowards(transform.position, puntoMov, speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, puntoMov) == 0)
                {
                    if(new Vector3(puntoMov.x, puntoMov.y, 0) == endPoint.transform.position) { 
                        _isEnding = true; 
                        pausa_final = true;
                        timer = tempsEsperaFinal; 
                        nextPoint = new Vector3(endPoint.transform.position.x, endPoint.transform.position.y + desplazamiento, 0);
                    }

                    if (impactat && impacte_act) { executarImpacte(puntoImpacto); impactat = false; }

                    moviendo = false;
                    skiing = false;
                    ice_trail.Stop(); 
                    speed = speed_snow;
                }
            }

            if ((right != 0 || top != 0) && !moviendo)
            {
                input.x = right;
                input.y = top;
                //animacions
                _animator.SetFloat("moveX", input.x);
                _animator.SetFloat("moveY", input.y);

                iniciarNuevoMovimiento();
            }

            right = top = 0;

            _animator.SetBool("isMoving", moviendo);
            _animator.SetBool("isSkiing", skiing);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(puntoMov + offset, radioCirculo);
    }

    public void onLeft()
    {
        right = -1; 
        top = 0;
    }
    public void onRight()
    {
        right = 1; 
        top = 0;
    }
    public void onBottom()
    {
        right = 0;
        top = -1;
    }
    public void onTop()
    {
        right = 0;
        top = 1;
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) Pause();
    }

    public void Pause()
    {
        _isPaused = !_isPaused;
        if (_isPaused)
        {
            menu.SetActive(true);
            Time.timeScale = 0f;    //pauso el juego 
        }
        else
        {
            menu.SetActive(false);
            Time.timeScale = 1f;    //pongo el juego en ejecucion   //velocidad de ejecucion. si ho poso a 0.5 anira a camarea lenta
        }

    }
}
