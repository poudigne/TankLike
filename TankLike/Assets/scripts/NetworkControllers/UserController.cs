using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UserController : MonoBehaviour
{
  public Transform loginForm;
  public Transform registerForm;
  public Transform forgotPassword;

  private const string GAME_NAME = "TANK";
  private const string INPUT_USERNAME_TAG = "input_username";
  private const string INPUT_PASSWORD_TAG = "input_password";
  private const string INPUT_FIRSTNAME_TAG = "input_firstname";
  private const string INPUT_LASTNAME_TAG = "input_lastname";
  private const string INPUT_CONFIRMPASSWORD_TAG = "input_confirmpassword";
  private const string INPUT_EMAIL_TAG = "input_email";

  void Awake()
  {
  }

  public void Login()
  {


    Debug.Log("Login in");
  }

  public void ForgotMyPassword()
  {
    Debug.Log("Forgot my password");
  }

  public void RegisterAccount()
  {
    //RegisterInformations registerInformations = new RegisterInformations()
    //{
    //  firstname = GetFieldValueWithTag(registerForm,INPUT_FIRSTNAME_TAG),
    //  lastname = GetFieldValueWithTag(registerForm,INPUT_LASTNAME_TAG),
    //  username = GetFieldValueWithTag(registerForm, INPUT_USERNAME_TAG),
    //  email = GetFieldValueWithTag(registerForm,INPUT_EMAIL_TAG),
    //  password = GetFieldValueWithTag(registerForm,INPUT_PASSWORD_TAG),
    //  passwordConfirm = GetFieldValueWithTag(registerForm,INPUT_CONFIRMPASSWORD_TAG),
    //};

    string errorMessage = string.Empty;
    //if (!ValidateRegisterForm(registerInformations, out errorMessage))
    //{
    //  Debug.Log("Something went wrong. Error : " + errorMessage);
    //  return;
    //}

    //RemotingFacade.GetInstance().SendRegisterAccountRequest(registerInformations);
  }

  #region Form validation
  //static bool ValidateRegisterForm(RegisterInformations registerInformations, out string errorMessage)
  //{
  //  if (!registerInformations.FieldHasValue())
  //  {
  //    errorMessage = "Somefield are invalid.";
  //    return false;
  //  }

  //  if (!registerInformations.IsPasswordValid())
  //  {
  //    errorMessage = "Password is invalid.";
  //    return false;
  //  }

  //  if (!registerInformations.IsPasswordMath())
  //  {
  //    errorMessage = "Passwords doesn't match.";
  //    return false;
  //  }
  //  errorMessage = string.Empty;
  //  return true;
  //}
  #endregion

  string GetFieldValueWithTag(Transform registerForm, string tag)
  {
    InputField[] fields = registerForm.GetComponentsInChildren<InputField>();
    foreach (var inputField in fields.Where(inputField => inputField.tag == tag))
    {
      return inputField.text;
    }
    return string.Empty;
  }

  #region Form navigation
  public void DisplayRegisterForm()
  {
    Debug.Log("Displaying register form");
    loginForm.gameObject.SetActive(false);
    forgotPassword.gameObject.SetActive(false);
    registerForm.gameObject.SetActive(true);
  }

  public void DisplayLoginForm()
  {
    Debug.Log("Displaying login form");
    loginForm.gameObject.SetActive(true);
    forgotPassword.gameObject.SetActive(false);
    registerForm.gameObject.SetActive(false);
  }
  public void DisplayForgotPasswordForm()
  {
    Debug.Log("Displaying login form");
    loginForm.gameObject.SetActive(false);
    forgotPassword.gameObject.SetActive(true);
    registerForm.gameObject.SetActive(false);
  }
  #endregion

 
}
