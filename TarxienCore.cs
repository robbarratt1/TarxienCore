using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.IO;

public class TarxienCore : MonoBehaviour
{
    //Declarations
    #region

    public GameObject sun, moon, alcyone, atlas, electra, maia, merope, taygeta,
        pleione, celaeno, sterope, alphaCrux, betaCrux, gammaCrux, deltaCrux, sirius,
        canopus, arcturus, vega, rigel, procyon, achernar, betelguese, sphere;
    public GameObject target1, target2, target3, target4, model;
    public Slider year, month, day, hour, minute;
    public Dropdown astroChoice;
    public InputField latitudeInput, longitudeInput, heightInput, horizonInput, oriField, nameField;
    public Text date, yearText, monthText, dayText, hourText, minuteText, moonText;
    public GameObject target;
    public Button saveButton;

    private List<GameObject> pleiades = new List<GameObject>();
    private List<GameObject> crux = new List<GameObject>();
    private double actualYear;
    private GameObject activeAstro;
    private List<GameObject> activeAstroList = new List<GameObject>();
    private double dayOfYear;
    private double latitude, longitude, targetOri;
    private double earthAxis, trueAxis;
    private double jde;
    private double deltaPsi, deltaEpsilon;
    private double rightAscension, declination, apparentAscension, apparentDeclination;
    private double altitude, azimuth, height;
    private double zeta, eta, theta;
    private double earthToSun, scalingFactor = 1000;
    private bool refraction = true;
    private double sunTrueLongitude, trueAnomaly, omega, apparentLongitude, meanLongitude;

    private double moonLongitude, moonElongation, moonAnomaly, moonArgumentOfLatitude, moonA1, moonA2, moonA3, sunMeanAnomaly;
    private double[,] term1 = new double[60, 6];
    private double[,] term2 = new double[60, 5];
    private double apparentMoonLong;

    private List<MasterList> astroData = new List<MasterList>();
    private double astroNumber;
    private List<GameObject> spheres = new List<GameObject>();

    private List<string[]> rowData = new List<string[]>();

    private double absoluteAltitude, absoluteDistance, targetHorizon, timeRising, timeSetting;

    public GameObject emptyObj;

    private string dateSumSol, dateWinSol, dateSprEq, dateAutEq, oriSumSol, oriWinSol, oriSprEq, oriAutEq;
    private double jdeSumSol, jdeWinSol, jdeSprEq, jdeAutEq;
    private double tempLon = 1000;
    private double tempNum = 10000;
    private List<double> jdeList = new List<double>();
    private double ascOri, desOri;
    private double closestAsc, closestDes;
    private List<double[]> absolutes = new List<double[]>();
    private bool savCon = false;
    private double heightChange = 0;
    private string siteName = "";
    Vector3 location1, location2;


    #endregion

    //Various
    #region

    private void Start()
    {
        CreateObjects();
        latitudeInput.text = "35.826638";
        longitudeInput.text = "-14.436305";
        heightInput.text = "86";
        horizonInput.text = "4";
        oriField.text = "92";
        nameField.text = "Mnajdra South";
        CreateMoonTerms();
        CreateAstroData();
        saveButton.onClick.AddListener(ButtonPressed);
    }

    private void Update()
    {
        actualYear = year.value;
        ChooseAstro();
        UpdateDisplay();
        Calculations();
        DrawLine();
        UpdateSiteName();
        AssignSiteHeight();
        QuickSave();

    }

    void DrawLine()
    {
        if (activeAstroList.Count == 0)
        {
            Debug.DrawRay(activeAstro.transform.position,
                                target.transform.position - activeAstro.transform.position, Color.red);
        } else foreach (GameObject act in activeAstroList)
            {
                Debug.DrawRay(act.transform.position, target.transform.position - act.transform.position, Color.red);
            }
    }

    void UpdateDisplay()
    {
        string tempYear = "";

        if (actualYear < 2000)
        {
            tempYear = (2000 - actualYear).ToString() + " AD";
        } else
        {
            tempYear = (actualYear - 2000).ToString() + " BC";
        }

        yearText.text = "Year: " + tempYear;

        string monthName = "";

        if (month.value == 1)
        {
            day.maxValue = 31;
            monthName = "January";
        } else if (month.value == 2)
        {
            day.maxValue = 28;
            if (day.value > 28)
            {
                day.value = 28;
            }
            monthName = "February";
        } else if (month.value == 3)
        {
            day.maxValue = 31;
            monthName = "March";
        } else if (month.value == 4)
        {
            day.maxValue = 30;
            if (day.value > 30)
            {
                day.value = 30;
            }
            monthName = "April";
        } else if (month.value == 5)
        {
            day.maxValue = 31;
            monthName = "May";
        } else if (month.value == 6)
        {
            day.maxValue = 30;
            if (day.value > 30)
            {
                day.value = 30;
            }
            monthName = "June";
        } else if (month.value == 7)
        {
            day.maxValue = 31;
            monthName = "July";
        } else if (month.value == 8)
        {
            day.maxValue = 31;
            monthName = "August";
        } else if (month.value == 9)
        {
            day.maxValue = 30;
            if (day.value > 30)
            {
                day.value = 30;
            }
            monthName = "September";
        } else if (month.value == 10)
        {
            day.maxValue = 31;
            monthName = "October";
        } else if (month.value == 11)
        {
            day.maxValue = 30;
            if (day.value > 30)
            {
                day.value = 30;
            }
            monthName = "November";
        } else if (month.value == 12)
        {
            day.maxValue = 31;
            monthName = "December";
        }

        monthText.text = monthName;
        dayText.text = "Day: " + day.value.ToString();
        hourText.text = "Hour: " + hour.value.ToString();
        minuteText.text = "Minute: " + minute.value.ToString();

        string tempMonth = month.value.ToString();
        if (month.value < 10)
        {
            tempMonth = "0" + month.value;
        }

        string tempDay = day.value.ToString();
        if (day.value < 10)
        {
            tempDay = "0" + day.value;
        }

        string tempHour = hour.value.ToString();
        if (hour.value < 10)
        {
            tempHour = "0" + hour.value;
        }

        string tempMinute = minute.value.ToString();
        if (minute.value < 10)
        {
            tempMinute = "0" + minute.value;
        }


        date.text = tempDay + "/" + tempMonth + "/" + tempYear +
            " - " + tempHour + ":" + tempMinute;

    }

    void ChooseAstro()
    {
        moonText.gameObject.SetActive(false);

        if (astroChoice.value == 0)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = sun;
            astroNumber = 11;
            SphereAssoc();
        } else if (astroChoice.value == 1)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = moon;
            astroNumber = 12;
            SphereAssoc();
            moonText.gameObject.SetActive(true);
        } else if (astroChoice.value == 2)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = null;
            activeAstroList.Add(alcyone);
            activeAstroList.Add(atlas);
            activeAstroList.Add(electra);
            activeAstroList.Add(maia);
            activeAstroList.Add(merope);
            activeAstroList.Add(taygeta);
            activeAstroList.Add(pleione);
            activeAstroList.Add(celaeno);
            activeAstroList.Add(sterope);
            astroNumber = 1;
            SphereAssoc();
        }
        else if (astroChoice.value == 3)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = null;
            activeAstroList.Add(alphaCrux);
            activeAstroList.Add(betaCrux);
            activeAstroList.Add(gammaCrux);
            activeAstroList.Add(deltaCrux);
            astroNumber = 2;
            SphereAssoc();
        }
        else if (astroChoice.value == 4)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = sirius;
            astroNumber = 3;
            SphereAssoc();
        }
        else if (astroChoice.value == 5)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = canopus;
            astroNumber = 4;
            SphereAssoc();
        }
        else if (astroChoice.value == 6)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = arcturus;
            astroNumber = 5;
            SphereAssoc();
        }
        else if (astroChoice.value == 7)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = vega;
            astroNumber = 6;
            SphereAssoc();
        }
        else if (astroChoice.value == 8)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = rigel;
            astroNumber = 7;
            SphereAssoc();
        }
        else if (astroChoice.value == 9)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = procyon;
            astroNumber = 8;
            SphereAssoc();
        }
        else if (astroChoice.value == 10)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = achernar;
            astroNumber = 9;
            SphereAssoc();
        }
        else if (astroChoice.value == 11)
        {
            DeleteSpheres();
            spheres.Clear();
            activeAstroList.Clear();
            activeAstro = betelguese;
            astroNumber = 10;
            SphereAssoc();
        }

    }

    void CreateObjects()
    {
        pleiades.Add(alcyone);
        pleiades.Add(atlas);
        pleiades.Add(electra);
        pleiades.Add(maia);
        pleiades.Add(merope);
        pleiades.Add(taygeta);
        pleiades.Add(pleione);
        pleiades.Add(celaeno);
        pleiades.Add(sterope);

        crux.Add(alphaCrux);
        crux.Add(betaCrux);
        crux.Add(gammaCrux);
        crux.Add(deltaCrux);

    }

    void Calculations()
    {
        jde = CalculateJD();
        CalculateDay();
        CalculateEarthAxis();
        SiteHeight();
        ParseFields();


        if (activeAstro == sun)
        {
            SunCalculations();
        }
        if (activeAstro == moon)
        {
            MoonCalculations();
        }

        if (astroNumber > 0 && astroNumber < 11)
        {
            CalculateStar();
        }
    }

    void ParseFields()
    {
        double number;
        if (double.TryParse(latitudeInput.text, out number))
        {
            latitude = double.Parse(latitudeInput.text);
        }
        if (double.TryParse(longitudeInput.text, out number))
        {
            longitude = double.Parse(longitudeInput.text);
        }
        if (double.TryParse(horizonInput.text, out number))
        {
            targetHorizon = double.Parse(horizonInput.text);
        }
        if (double.TryParse(oriField.text, out number))
        {
            targetOri = double.Parse(oriField.text);
        }

    }

    void CalculateDay()
    {
        if (month.value == 1)
        {
            dayOfYear = day.value;
        } else if (month.value == 2)
        {
            dayOfYear = day.value + 31;
        }
        else if (month.value == 3)
        {
            dayOfYear = day.value + 58;
        } else if (month.value == 4)
        {
            dayOfYear = day.value + 90;
        }
        else if (month.value == 5)
        {
            dayOfYear = day.value + 120;
        } else if (month.value == 6)
        {
            dayOfYear = day.value + 151;
        }
        else if (month.value == 7)
        {
            dayOfYear = day.value + 181;
        } else if (month.value == 8)
        {
            dayOfYear = day.value + 212;
        }
        else if (month.value == 9)
        {
            dayOfYear = day.value + 243;
        }
        else if (month.value == 10)
        {
            dayOfYear = day.value + 273;
        }
        else if (month.value == 11)
        {
            dayOfYear = day.value + 304;
        } else if (month.value == 12)
        {
            dayOfYear = day.value + 334;
        }

    }

    void CalculateEarthAxis()
    {
        double t = ((jde - 2451545f) / 36525f) / 100f;
        earthAxis = 23.43929f - 1.300258f * t - 0.0004305556f * Math.Pow(t, 2f) + 0.5553472f * Math.Pow(t, 3f) -
            0.01427222f * Math.Pow(t, 4f) - 0.06935278f * Math.Pow(t, 5f) - 0.01084722f * Math.Pow(t, 6f) +
            0.001977778f * Math.Pow(t, 7f) + 0.007741667f * Math.Pow(t, 8f) + 0.001608333f * Math.Pow(t, 9f) +
            0.0006805556f * Math.Pow(t, 10f);

        CalculateNutation();
    }

    double CalculateJD()
    {
        double tempMonth, tempYear, jdeTemp, tempDay;

        tempDay = day.value + hour.value / 24 + (minute.value / 60) / 24;

        tempYear = 2000 - (year.value);

        if (month.value > 2)
        {
            tempMonth = month.value;
        } else
        {
            tempYear = tempYear - 1;
            tempMonth = month.value + 12;
        }

        if ((2000 - (year.value)) > 1582) {
            double a = Math.Floor(tempYear / 100);
            double b = 2 - a + Math.Floor(a / 4);
            jdeTemp = Math.Floor(365.25f * (tempYear + 4716f)) +
                Math.Floor(30.6001f * (tempMonth + 1)) + tempDay + b - 1524.5f;
        }
        else if ((2000 - (year.value)) < 1582)
        {
            double y = 2000 - (year.value);
            double d = Math.Floor(y / 100) - Math.Floor(y / 400) - 2;

            jdeTemp = Math.Floor(365.25f * (tempYear + 4716f)) +
                Math.Floor(30.6001f * (tempMonth + 1)) + (tempDay - d) - 1524.5f;

        }
        else
        {
            jdeTemp = 0f;
        }

        return jdeTemp;
    }

    void CalculateNutation()
    {
        double t = (jde - 2451545f) / 36525;
        double omega = Normalise(125.04452f - (1934.136261f * t));
        double sunLon = Normalise(280.4665f + (36000.7698f * t));
        double moonLon = Normalise(218.3165f + (481267.8813f * t));

        deltaPsi = (-0.004777778f * Math.Sin(Mathf.Rad2Deg * (omega))) - (0.0003666667f * Math.Sin(Mathf.Rad2Deg * 2 * sunLon)) -
            (0.00006388889f * Math.Sin(Mathf.Rad2Deg * 2 * moonLon)) + (0.00005833333f * Math.Sin(Mathf.Rad2Deg * 2 * omega));
        deltaEpsilon = (0.002555556f * Math.Cos(Mathf.Rad2Deg * omega)) + (0.0001583333f * Math.Cos(Mathf.Rad2Deg * 2 * sunLon)) +
            (0.00002777778f * Math.Cos(Mathf.Rad2Deg * 2 * moonLon)) - (0.000025f * Math.Cos(Mathf.Rad2Deg * 2 * omega));

        trueAxis = earthAxis + deltaEpsilon;

    }

    void SiteHeight()
    {
        double h = float.Parse(heightInput.text);

        height = h;

    }

    void ChangeOrientation()
    {


        location1 = new Vector3((target1.transform.position.x + target2.transform.position.x) / 2,
            (target1.transform.position.y + target2.transform.position.y) / 2, (target1.transform.position.z +
            target2.transform.position.z) / 2);

        location2 = new Vector3((target3.transform.position.x + target4.transform.position.x) / 2,
            (target3.transform.position.y + target4.transform.position.y) / 2, (target3.transform.position.z +
            target4.transform.position.z) / 2);

        double modelDeg = (Math.Atan2(location2.x - location1.x, location2.z - location1.z) * 180) / Math.PI;


        if (!emptyObj) {

            emptyObj = new GameObject();
        }

        emptyObj.transform.position = location2;

        model.transform.parent = emptyObj.transform;

        emptyObj.transform.position = new Vector3(0, (target3.transform.position.y + target4.transform.position.y) / 2, 0);

        target.transform.position = new Vector3(0, ((target3.transform.position.y + target4.transform.position.y) / 2) + (float)heightChange, 0);

        emptyObj.transform.rotation = Quaternion.AngleAxis((float)-modelDeg + (float)targetOri, Vector3.up);



    }

    void AssignSiteHeight()
    {
        if (scalingFactor > 0)
        {

            model.transform.position = new Vector3(model.transform.position.x, (float)(height / scalingFactor), model.transform.position.z);
            heightChange = height / scalingFactor;
            ChangeOrientation();
        } else
        {
            model.transform.position = new Vector3(model.transform.position.x, 0, model.transform.position.z);
            heightChange = 0;
            ChangeOrientation();
        }
    }

    void UpdateSiteName()
    {
        siteName = nameField.text;
    }

    #endregion

    //Sun Calculations
    #region

    void SunCalculations()
    {

        double jDate = (jde - 2451545) / 36525;

        apparentAscension = CalculateAscension(jDate);
        apparentDeclination = CalculateDeclination(jDate);

        double greenTime = Normalise(280.46061837 + 360.98564736629 * (jde - 2451545) + 0.000387933 * Math.Pow(jDate, 2) -
            Math.Pow(jDate, 3) / 38710000 + deltaPsi * Math.Cos(Mathf.Deg2Rad * trueAxis));
        double t = jDate / 10;

        double newMeanLon = Normalise(280.4664567 + (360007.6982279 * t) + (0.03032028 * Math.Pow(t, 2)) +
            (Math.Pow(t, 3) / 49931) - (Math.Pow(t, 4) / 15299) - (Math.Pow(t, 5) / 1988000));

        double equationOfTime = Normalise(newMeanLon - apparentAscension + deltaPsi * Math.Cos(Mathf.Deg2Rad * trueAxis));

        double hourAngle = Normalise(greenTime - longitude - apparentAscension + equationOfTime);


        azimuth = Mathf.Rad2Deg * Math.Atan2(Math.Sin(Mathf.Deg2Rad * hourAngle), Math.Cos(Mathf.Deg2Rad * hourAngle) *
            Math.Sin(Mathf.Deg2Rad * latitude) - Math.Tan(Mathf.Deg2Rad * apparentDeclination) * Math.Cos(Mathf.Deg2Rad * latitude));

        altitude = Mathf.Rad2Deg * Math.Asin(Math.Sin(Mathf.Deg2Rad * latitude) * Math.Sin(Mathf.Deg2Rad * apparentDeclination) +
            Math.Cos(Mathf.Deg2Rad * latitude) * Math.Cos(Mathf.Deg2Rad * apparentDeclination) * Math.Cos(Mathf.Deg2Rad * hourAngle));

        double tempAz = azimuth + 180;

        azimuth = -(tempAz + 90);


        absoluteAltitude = altitude;

        scalingFactor = 5000;
        double earthToSun = (149597870 - 0.01672 * Math.Cos(Mathf.Deg2Rad * 0.9856 * (day.value - 4))) / scalingFactor;

        absoluteDistance = earthToSun;

        if (refraction == true)
        {
            altitude = CalculateRefraction(altitude);
        }

        Vector3 pos = SphericalToCartesian(earthToSun, azimuth, altitude);

        sun.transform.position = pos;

        CheckSolEqui(apparentLongitude);
    }

    double Normalise(double input)
    {
        double tempDouble = input;

        if (input > 360 || input < -360)
        {
            tempDouble = input % 360;

            if (tempDouble < 0)
            {
                tempDouble = tempDouble + 360;

            }


            if (tempDouble == 0)
            {
                tempDouble = 360;

            }

        }

        return tempDouble;

    }

    double SameSign(double a, double b)
    {
        if (b > 0 && a > 0)
        {
            return a;
        }
        else if (b > 0 && a < 0)
        {
            return -a;
        }
        else if (b <= 0 && a <= 0)
        {
            return a;
        }
        else
        {
            return -a;
        }

    }

    Vector3 SphericalToCartesian(double radius, double polar, double elevation)
    {
        double a = radius * Math.Cos(Mathf.Deg2Rad * elevation);
        Vector3 outCart;
        outCart.x = (float)(a * Math.Cos(Mathf.Deg2Rad * polar));
        outCart.y = (float)(radius * Math.Sin(Mathf.Deg2Rad * elevation));
        outCart.z = (float)(a * Math.Sin(Mathf.Deg2Rad * polar));

        return outCart;
    }

    double CalculateRefraction(double alt)
    {
        return alt + (1 / Mathf.Rad2Deg * (Math.Tan(Mathf.Deg2Rad * (altitude + (7.31 / (altitude + 4.4))))));

    }

    void CalculateSunData(double date)
    {
        double t = date;

        meanLongitude = Normalise(280.46646 + 36000.76983 * t + 0.0003032 * Math.Pow(t, 2));
        double meanAnomaly = Normalise(357.52910 + 35999.05030 * t -
            0.0001559 * Math.Pow(t, 2) - 0.00000048 * Math.Pow(t, 3));
        double earthEccentricity = 0.016708617 - 0.000042037 * t - 0.0000001236 * Math.Pow(t, 2);
        double sunCentre = (1.914600 - 0.004817 * t - 0.000014 * Math.Pow(t, 2)) * Math.Sin(Mathf.Deg2Rad * meanAnomaly) +
            (0.019993 - 0.000101 * t) * Math.Sin(Mathf.Deg2Rad * (2 * meanAnomaly)) +
            0.000290 * Math.Sin(Mathf.Deg2Rad * (3 * meanAnomaly));

        sunTrueLongitude = meanLongitude + sunCentre;
        trueAnomaly = meanAnomaly + sunCentre;

        double sunRadius = (1.000001018 * (1 - Math.Pow(earthEccentricity, 2))) /
            (1 + earthEccentricity * Math.Cos(Mathf.Deg2Rad * trueAnomaly));

        omega = 125.04 - 1934.136 * t;
        apparentLongitude = Normalise(sunTrueLongitude - 0.00569 - 0.00478 * Math.Sin(Mathf.Deg2Rad * omega));


    }

    void CheckSolEqui(double apparentLon)
    {

        if (apparentLon > 359.7 || apparentLon < 0.3)
        {
            if (Math.Abs(360 - apparentLon) < 2)
            {
                if (Math.Abs(360 - tempLon) > Math.Abs(360 - apparentLon))
                {
                    tempLon = apparentLon;
                    dateSprEq = day.value.ToString() + "/" + month.value.ToString() +
                        " - " + hour.value.ToString() + ":" + minute.value.ToString();
                    jdeSprEq = Math.Floor(jde);
                }
            }
            else if (Math.Abs(apparentLon) < 2)
            {
                if (Math.Abs(tempLon) > Math.Abs(apparentLon))
                {
                    tempLon = apparentLon;
                    dateSprEq = day.value.ToString() + "/" + month.value.ToString() +
                        " - " + hour.value.ToString() + ":" + minute.value.ToString();
                    jdeSprEq = Math.Floor(jde);
                }
            }
        }

        if (apparentLon > 89.7 && apparentLon < 90.3)
        {

            if (Math.Abs(90 - apparentLon) < 2)
            {
                if (Math.Abs(90 - tempLon) > Math.Abs(90 - apparentLon))
                {
                    tempLon = apparentLon;
                    dateSumSol = day.value.ToString() + "/" + month.value.ToString() +
                        " - " + hour.value.ToString() + ":" + minute.value.ToString();
                    jdeSumSol = Math.Floor(jde);
                }
            }
        }

        if (apparentLon > 179.7 && apparentLon < 180.3)
        {

            if (Math.Abs(180 - apparentLon) < 2)
            {
                if (Math.Abs(180 - tempLon) > Math.Abs(90 - apparentLon))
                {
                    tempLon = apparentLon;
                    dateAutEq = day.value.ToString() + "/" + month.value.ToString() +
                        " - " + hour.value.ToString() + ":" + minute.value.ToString();
                    jdeAutEq = Math.Floor(jde);
                }
            }
        }

        if (apparentLon > 269.7 && apparentLon < 270.3)
        {
            if (Math.Abs(270 - apparentLon) < 2)
            {
                if (Math.Abs(270 - tempLon) > Math.Abs(270 - apparentLon))
                {
                    tempLon = apparentLon;
                    dateWinSol = day.value.ToString() + "/" + month.value.ToString() +
                        " - " + hour.value.ToString() + ":" + minute.value.ToString();
                    jdeWinSol = Math.Floor(jde);
                }
            }
        }
    }

    double CalculateAscension(double date)
    {
        CalculateSunData(date);

        rightAscension = Mathf.Rad2Deg * Math.Atan2(Math.Cos(Mathf.Deg2Rad * (trueAxis + (0.00256 * Math.Cos(Mathf.Deg2Rad * omega)))) *
            Math.Sin(Mathf.Deg2Rad * apparentLongitude), Math.Cos(Mathf.Deg2Rad * apparentLongitude));

        rightAscension = Normalise(rightAscension);

        if (rightAscension < 0)
        {
            rightAscension += 360;

        }

        return rightAscension;

    }

    double CalculateDeclination(double date)
    {
        CalculateSunData(date);

        declination = Math.Asin(Math.Sin(Mathf.Deg2Rad * trueAxis) * Math.Sin(Mathf.Deg2Rad * sunTrueLongitude));

        double tempApparentDeclination = Mathf.Rad2Deg * Math.Asin(Math.Sin(Mathf.Deg2Rad * trueAxis +
            Mathf.Deg2Rad * 0.00256 * Math.Cos(Mathf.Deg2Rad * omega)) *
            Math.Sin(Mathf.Deg2Rad * apparentLongitude));

        return tempApparentDeclination;
    }
    #endregion

    //Moon Calculations
    #region

    void MoonCalculations()
    {
        double t = (jde - 2451545.0) / 36525;
        moonLongitude = Normalise(218.3164591 + 481267.88134236 * t - 0.0013268 * Math.Pow(t, 2) +
            Math.Pow(t, 3) / 538841 - Math.Pow(t, 4) / 65194000);

        moonElongation = Normalise(297.8502042 + 445267.1115168 * t - 0.00016300 * Math.Pow(t, 2) +
            Math.Pow(t, 3) / 545868 - Math.Pow(t, 4) / 113065000);

        sunMeanAnomaly = Normalise(357.5291092 + 35999.0502909 * t - 0.0001536 * Math.Pow(t, 2) +
            Math.Pow(t, 3) / 24490000);

        moonAnomaly = Normalise(134.9634114 + 477198.8676313 * t + 0.0089970 * Math.Pow(t, 2) +
            Math.Pow(t, 3) / 69699 - Math.Pow(t, 4) / 863310000);

        moonArgumentOfLatitude = Normalise(93.2720993 + 483202.0175273 * t - 0.0034029 * Math.Pow(t, 2) -
            Math.Pow(t, 3) / 3526000 + Math.Pow(t, 4) / 863310000);

        moonA1 = Normalise(119.75 + 131.849 * t);
        moonA2 = Normalise(53.09 + 479264.290 * t);
        moonA3 = Normalise(313.45 + 481266.484 * t);

        double sumL = 0, sumR = 0, sumB = 0;
        double ecc;

        ecc = 1 - 0.002516 * t - 0.0000074 * Math.Pow(t, 2);

        for (int a = 0; a < 60; a++)
        {
            if (term1[a, 1] == 1 || term1[a, 1] == -1)
            {
                sumL += term1[a, 4] * Math.Sin(Mathf.Deg2Rad * (term1[a, 0] * moonElongation + term1[a, 1] *
                    sunMeanAnomaly * ecc + term1[a, 2] * moonAnomaly + term1[a, 3] * moonArgumentOfLatitude));
            }
            else if (term1[a, 1] == 2 || term1[a, 1] == -2)
            {
                sumL += term1[a, 4] * Math.Sin(Mathf.Deg2Rad * (term1[a, 0] * moonElongation + term1[a, 1] *
                    sunMeanAnomaly * Math.Pow(ecc, 2) + term1[a, 2] * moonAnomaly + term1[a, 3] * moonArgumentOfLatitude));
            } else if (term1[a, 1] == 0)
            {
                sumL += term1[a, 4] * Math.Sin(Mathf.Deg2Rad * (term1[a, 0] * moonElongation +
                    term1[a, 2] * moonAnomaly + term1[a, 3] * moonArgumentOfLatitude));
            }
        }

        sumL = sumL + 3958 * Math.Sin(Mathf.Deg2Rad * moonA1) + 1962 * Math.Sin(Mathf.Deg2Rad * (moonLongitude - moonArgumentOfLatitude)) +
            318 * Math.Sin(Mathf.Deg2Rad * moonA2);

        for (int a = 0; a < 60; a++)
        {
            if (term1[a, 1] == 1 || term1[a, 1] == -1)
            {
                sumR += term1[a, 5] * Math.Cos(Mathf.Deg2Rad * (term1[a, 0] * moonElongation + term1[a, 1] *
                    sunMeanAnomaly * ecc + term1[a, 2] * moonAnomaly + term1[a, 3] * moonArgumentOfLatitude));
            }
            else if (term1[a, 1] == 2 || term1[a, 1] == -2)
            {
                sumR += term1[a, 5] * Math.Cos(Mathf.Deg2Rad * (term1[a, 0] * moonElongation + term1[a, 1] *
                    sunMeanAnomaly * Math.Pow(ecc, 2) + term1[a, 2] * moonAnomaly + term1[a, 3] * moonArgumentOfLatitude));
            }
            else if (term1[a, 1] == 0)
            {
                sumR += term1[a, 5] * Math.Cos(Mathf.Deg2Rad * (term1[a, 0] * moonElongation +
                    term1[a, 2] * moonAnomaly + term1[a, 3] * moonArgumentOfLatitude));
            }
        }

        for (int a = 0; a < 60; a++)
        {
            if (term2[a, 1] == 1 || term2[a, 1] == -1)
            {
                sumB += term2[a, 4] * Math.Sin(Mathf.Deg2Rad * (term2[a, 0] * moonElongation + term2[a, 1] *
                    sunMeanAnomaly * ecc + term2[a, 2] * moonAnomaly + term2[a, 3] * moonArgumentOfLatitude));
            }
            else if (term2[a, 1] == 2 || term2[a, 1] == -2)
            {
                sumB += term2[a, 4] * Math.Sin(Mathf.Deg2Rad * (term2[a, 0] * moonElongation + term2[a, 1] *
                    sunMeanAnomaly * Math.Pow(ecc, 2) + term2[a, 2] * moonAnomaly + term2[a, 3] * moonArgumentOfLatitude));
            }
            else if (term2[a, 1] == 0)
            {
                sumB += term2[a, 4] * Math.Sin(Mathf.Deg2Rad * (term2[a, 0] * moonElongation +
                    term2[a, 2] * moonAnomaly + term2[a, 3] * moonArgumentOfLatitude));
            }
        }

        sumB += -2235 * Math.Sin(Mathf.Deg2Rad * moonLongitude) + 382 * Math.Sin(Mathf.Deg2Rad * moonA3) +
            175 * Math.Sin(Mathf.Deg2Rad * (moonA1 - moonArgumentOfLatitude)) +
            175 * Math.Sin(Mathf.Deg2Rad * (moonA1 + moonArgumentOfLatitude)) +
            127 * Math.Sin(Mathf.Deg2Rad * (moonLongitude - moonAnomaly)) -
            115 * Math.Sin(Mathf.Deg2Rad * (moonLongitude + moonAnomaly));

        double lambda = moonLongitude + sumL / 1000000;
        double beta = sumB / 1000000;
        double delta = 385000.56 + sumR / 1000;

        apparentMoonLong = lambda + deltaPsi;

        double moonRightAscension = Mathf.Rad2Deg * Math.Atan2(Math.Sin(Mathf.Deg2Rad * apparentMoonLong) * Math.Cos(Mathf.Deg2Rad * trueAxis) -
            Math.Tan(Mathf.Deg2Rad * beta) * Math.Sin(Mathf.Deg2Rad * trueAxis), Math.Cos(Mathf.Deg2Rad * apparentMoonLong));

        double moonDeclination = Mathf.Rad2Deg * Math.Asin(Math.Sin(Mathf.Deg2Rad * beta) * Math.Cos(Mathf.Deg2Rad * trueAxis) +
            Math.Cos(Mathf.Deg2Rad * beta) * Math.Sin(Mathf.Deg2Rad * trueAxis) * Math.Sin(Mathf.Deg2Rad * apparentMoonLong));

        if (moonRightAscension < 0) {
            moonRightAscension += 360;
        }

        double greenTime = Normalise(280.46061837 + 360.98564736629 * (jde - 2451545) + 0.000387933 * Math.Pow(t, 2) -
             Math.Pow(t, 3) / 38710000 + deltaPsi * Math.Cos(Mathf.Deg2Rad * trueAxis));

        double hourAngle = Normalise(greenTime - longitude - moonRightAscension);

        double moonAzimuth = Mathf.Rad2Deg * Math.Atan2(Math.Sin(Mathf.Deg2Rad * hourAngle), Math.Cos(Mathf.Deg2Rad * hourAngle) *
            Math.Sin(Mathf.Deg2Rad * latitude) - Math.Tan(Mathf.Deg2Rad * moonDeclination) * Math.Cos(Mathf.Deg2Rad * latitude));

        double moonAltitude = Mathf.Rad2Deg * Math.Asin(Math.Sin(Mathf.Deg2Rad * latitude) * Math.Sin(Mathf.Deg2Rad * moonDeclination) +
            Math.Cos(Mathf.Deg2Rad * latitude) * Math.Cos(Mathf.Deg2Rad * moonDeclination) * Math.Cos(Mathf.Deg2Rad * hourAngle));

        if (moonAzimuth < 0)
        {
            moonAzimuth += 360;
        }

        double tempAz = Normalise(moonAzimuth + 180);

        moonAzimuth = -(tempAz + 90);

        scalingFactor = 100;
        double earthToMoon = delta / scalingFactor;

        absoluteAltitude = moonAltitude;
        absoluteDistance = earthToMoon;

        if (refraction == true)
        {
            altitude = CalculateRefraction(moonAltitude);
        }

        Vector3 pos = SphericalToCartesian(earthToMoon, moonAzimuth, moonAltitude);

        moon.transform.position = pos;

        ClosestDec();
    }

    void CreateMoonTerms()
    {
        //term1
        #region
        #region
        term1[0, 2] = 1;
        term1[1, 0] = 2;
        term1[1, 2] = -1;
        term1[2, 0] = 2;
        term1[3, 2] = 2;
        term1[4, 1] = 1;
        term1[5, 3] = 2;
        term1[6, 0] = 2;
        term1[6, 2] = -2;
        term1[7, 0] = 2;
        term1[7, 1] = -1;
        term1[7, 2] = -1;
        term1[8, 0] = 2;
        term1[8, 2] = 1;
        term1[9, 0] = 2;
        term1[9, 1] = -1;
        term1[10, 1] = 1;
        term1[10, 2] = -1;
        term1[11, 0] = 1;
        term1[12, 1] = 1;
        term1[12, 2] = 1;
        term1[13, 0] = 2;
        term1[13, 3] = -2;
        term1[14, 2] = 1;
        term1[14, 3] = 2;
        term1[15, 2] = 1;
        term1[15, 3] = -2;
        term1[16, 0] = 4;
        term1[16, 2] = -1;
        term1[17, 2] = 3;
        term1[18, 0] = 4;
        term1[18, 2] = -2;
        term1[19, 0] = 2;
        term1[19, 1] = 1;
        term1[19, 2] = -1;
        term1[20, 0] = 2;
        term1[20, 1] = 1;
        term1[21, 0] = 1;
        term1[21, 2] = -1f;
        term1[22, 0] = 1;
        term1[22, 1] = 1;
        term1[23, 0] = 2;
        term1[23, 1] = -1;
        term1[23, 2] = 1;
        term1[24, 0] = 2;
        term1[24, 2] = 2;
        term1[25, 0] = 4;
        term1[26, 0] = 2;
        term1[26, 2] = -3;
        term1[27, 1] = 1;
        term1[27, 2] = -2;
        term1[28, 0] = 2;
        term1[28, 2] = -1;
        term1[28, 3] = 2;
        term1[29, 0] = 2;
        term1[29, 1] = -1;
        term1[29, 2] = -2;
        term1[30, 0] = 1;
        term1[30, 2] = 1;
        term1[31, 0] = 2;
        term1[31, 1] = -2;
        term1[32, 1] = 1;
        term1[32, 2] = 2;
        term1[33, 1] = 2;
        term1[34, 0] = 2;
        term1[34, 1] = -2;
        term1[34, 2] = -1;
        term1[35, 0] = 2;
        term1[35, 2] = 1;
        term1[35, 3] = -2;
        term1[36, 0] = 2;
        term1[36, 3] = 2;
        term1[37, 0] = 4;
        term1[37, 1] = -1;
        term1[37, 2] = -1;
        term1[38, 2] = 2;
        term1[38, 3] = 2;
        term1[39, 0] = 3;
        term1[39, 2] = -1;
        term1[40, 0] = 2;
        term1[40, 1] = 1;
        term1[40, 2] = 1;
        term1[41, 0] = 4;
        term1[41, 1] = -1;
        term1[41, 2] = -2;
        term1[42, 1] = 2;
        term1[42, 2] = -1;
        term1[43, 0] = 2;
        term1[43, 1] = 2;
        term1[43, 2] = -1;
        term1[44, 0] = 2;
        term1[44, 1] = 1;
        term1[44, 2] = -2;
        term1[45, 0] = 2;
        term1[45, 1] = -1;
        term1[45, 3] = -2;
        term1[46, 0] = 4;
        term1[46, 2] = 1;
        term1[47, 2] = 4;
        term1[48, 0] = 4;
        term1[48, 1] = -1;
        term1[49, 0] = 1;
        term1[49, 2] = -2;
        term1[50, 0] = 2;
        term1[50, 1] = 1;
        term1[50, 3] = -2;
        term1[51, 2] = 2;
        term1[51, 3] = -2;
        term1[52, 0] = 1;
        term1[52, 1] = 1;
        term1[52, 2] = 1;
        term1[53, 0] = 3;
        term1[53, 2] = -2;
        term1[54, 0] = 4;
        term1[54, 2] = -3;
        term1[55, 0] = 2;
        term1[55, 1] = -1;
        term1[55, 2] = 2;
        term1[56, 1] = 2;
        term1[56, 2] = 1;
        term1[57, 0] = 1;
        term1[57, 1] = 1;
        term1[57, 2] = -1;
        term1[58, 0] = 2;
        term1[58, 2] = 3;
        term1[59, 0] = 2;
        term1[59, 2] = -1;
        term1[59, 3] = -2;
        #endregion

        #region
        term1[0, 4] = 6288774;
        term1[0, 5] = -20905355;
        term1[1, 4] = 1274027;
        term1[1, 5] = -3699111;
        term1[2, 4] = 658314;
        term1[2, 5] = -2955968;
        term1[3, 4] = 213618;
        term1[3, 5] = -569925;
        term1[4, 4] = -185116;
        term1[4, 5] = 48888;
        term1[5, 4] = -114332;
        term1[5, 5] = -3149;
        term1[6, 4] = 58793;
        term1[6, 5] = 246158;
        term1[7, 4] = 57066;
        term1[7, 5] = -152138;
        term1[8, 4] = 53322;
        term1[8, 5] = -170733;
        term1[9, 4] = 45758;
        term1[9, 5] = -204586;
        term1[10, 4] = -40923;
        term1[10, 5] = -129620;
        term1[11, 4] = -34720;
        term1[11, 5] = 108743;
        term1[12, 4] = -30383;
        term1[12, 5] = 104755;
        term1[13, 4] = 15327;
        term1[13, 5] = 10321;
        term1[14, 4] = -12528;
        term1[15, 4] = 10980;
        term1[15, 5] = 79661;
        term1[16, 4] = 10675;
        term1[16, 5] = -34782;
        term1[17, 4] = 10034;
        term1[17, 5] = -23210;
        term1[18, 4] = 8548;
        term1[18, 5] = -21636;
        term1[19, 4] = -7888;
        term1[19, 5] = 24208;
        term1[20, 4] = -6766;
        term1[20, 5] = 30824;
        term1[21, 4] = -5163;
        term1[21, 5] = -8379;
        term1[22, 4] = 4987;
        term1[22, 5] = -16675;
        term1[23, 4] = 4036;
        term1[23, 5] = -12831;
        term1[24, 4] = 3994;
        term1[24, 5] = -10445;
        term1[25, 4] = 3861;
        term1[25, 5] = -11650;
        term1[26, 4] = 3665;
        term1[26, 5] = 14403;
        term1[27, 4] = -2689;
        term1[27, 5] = -7003;
        term1[28, 4] = -2602;
        term1[29, 4] = 2390;
        term1[29, 5] = 10056;
        term1[30, 4] = -2348;
        term1[30, 5] = 6322;
        term1[31, 4] = 2236;
        term1[31, 5] = -9884;
        term1[32, 4] = -2120;
        term1[32, 5] = 5751;
        term1[33, 4] = -2069;
        term1[34, 4] = 2048;
        term1[34, 5] = -4950;
        term1[35, 4] = -1773;
        term1[35, 5] = 4130;
        term1[36, 4] = -1595;
        term1[37, 4] = 1215;
        term1[37, 5] = -3958;
        term1[38, 4] = -1110;
        term1[39, 4] = -892;
        term1[39, 5] = 3258;
        term1[40, 4] = -810;
        term1[40, 5] = 2616;
        term1[41, 4] = 759;
        term1[41, 5] = -1897;
        term1[42, 4] = -713;
        term1[42, 5] = -2117;
        term1[43, 4] = -700;
        term1[43, 5] = 2354;
        term1[44, 4] = 691;
        term1[45, 4] = 596;
        term1[46, 4] = 549;
        term1[46, 5] = -1423;
        term1[47, 4] = 537;
        term1[47, 5] = -1117;
        term1[48, 4] = 520;
        term1[48, 5] = -1571;
        term1[49, 4] = -487;
        term1[49, 5] = -1739;
        term1[50, 4] = -399;
        term1[51, 4] = -381;
        term1[51, 5] = -4421;
        term1[52, 4] = 351;
        term1[53, 4] = -340;
        term1[54, 4] = 330;
        term1[55, 4] = 327;
        term1[56, 4] = -323;
        term1[56, 5] = 1165;
        term1[57, 4] = 299;
        term1[58, 4] = 294;
        term1[59, 5] = 8752;
        #endregion
        #endregion

        //term2
        #region
        #region
        term2[0, 3] = 1;
        term2[1, 2] = 1;
        term2[1, 3] = 1;
        term2[2, 2] = 1;
        term2[2, 3] = -1;
        term2[3, 0] = 2;
        term2[3, 3] = -1;
        term2[4, 0] = 2;
        term2[4, 2] = -1;
        term2[4, 3] = 1;
        term2[5, 0] = 2;
        term2[5, 2] = -1;
        term2[5, 3] = -1;
        term2[6, 0] = 2;
        term2[6, 3] = 1;
        term2[7, 2] = 2;
        term2[7, 3] = 1;
        term2[8, 0] = 2;
        term2[8, 2] = 1;
        term2[8, 3] = -1;
        term2[9, 2] = 2;
        term2[9, 3] = -1;
        term2[10, 0] = 2;
        term2[10, 1] = -1;
        term2[10, 3] = -1;
        term2[11, 0] = 2;
        term2[11, 2] = -2;
        term2[11, 3] = -1;
        term2[12, 0] = 2;
        term2[12, 2] = 1;
        term2[12, 3] = 1;
        term2[13, 0] = 2;
        term2[13, 1] = 1;
        term2[13, 3] = -1;
        term2[14, 0] = 2;
        term2[14, 1] = -1;
        term2[14, 2] = -1;
        term2[14, 3] = 1;
        term2[15, 0] = 2;
        term2[15, 1] = -1;
        term2[15, 3] = 1;
        term2[16, 0] = 2;
        term2[16, 1] = -1;
        term2[16, 2] = -1;
        term2[16, 3] = -1;
        term2[17, 1] = 1;
        term2[17, 2] = -1;
        term2[17, 3] = -1;
        term2[18, 0] = 4;
        term2[18, 2] = -1;
        term2[18, 3] = -1;
        term2[19, 1] = 1;
        term2[19, 3] = 1;
        term2[20, 3] = 3;
        term2[21, 1] = 1;
        term2[21, 2] = -1;
        term2[21, 3] = 1;
        term2[22, 0] = 1;
        term2[22, 3] = 1;
        term2[23, 1] = 1;
        term2[23, 2] = 1;
        term2[23, 3] = 1;
        term2[24, 1] = 1;
        term2[24, 2] = 1;
        term2[24, 3] = -1;
        term2[25, 1] = 1;
        term2[25, 3] = -1;
        term2[26, 0] = 1;
        term2[26, 3] = -1;
        term2[27, 2] = 3;
        term2[27, 3] = 1;
        term2[28, 0] = 4;
        term2[28, 3] = -1;
        term2[29, 0] = 4;
        term2[29, 2] = -1;
        term2[29, 3] = 1;
        term2[30, 2] = 1;
        term2[30, 3] = -3;
        term2[31, 0] = 4;
        term2[31, 2] = -2;
        term2[31, 3] = 1;
        term2[32, 0] = 2;
        term2[32, 3] = -3;
        term2[33, 0] = 2;
        term2[33, 2] = 2;
        term2[33, 3] = -1;
        term2[34, 0] = 2;
        term2[34, 1] = -1;
        term2[34, 2] = 1;
        term2[34, 3] = -1;
        term2[35, 0] = 2;
        term2[35, 2] = -2;
        term2[35, 3] = 1;
        term2[36, 2] = 3;
        term2[36, 3] = -1;
        term2[37, 0] = 2;
        term2[37, 2] = 2;
        term2[37, 3] = 1;
        term2[38, 0] = 2;
        term2[38, 2] = -3;
        term2[38, 3] = -1;
        term2[39, 0] = 2;
        term2[39, 1] = 1;
        term2[39, 2] = -1;
        term2[39, 3] = 1;
        term2[40, 0] = 2;
        term2[40, 1] = 1;
        term2[40, 3] = 1;
        term2[41, 0] = 4;
        term2[41, 3] = 1;
        term2[42, 0] = 2;
        term2[42, 1] = -1;
        term2[42, 2] = 1;
        term2[42, 3] = 1;
        term2[43, 0] = 2;
        term2[43, 1] = -2;
        term2[43, 3] = -1;
        term2[44, 2] = 1;
        term2[44, 3] = 3;
        term2[45, 0] = 2;
        term2[45, 1] = 1;
        term2[45, 2] = 1;
        term2[45, 3] = -1;
        term2[46, 0] = 1;
        term2[46, 1] = 1;
        term2[46, 3] = -1;
        term2[47, 0] = 1;
        term2[47, 1] = 1;
        term2[47, 3] = 1;
        term2[48, 1] = 1;
        term2[48, 2] = -2;
        term2[48, 3] = -1;
        term2[49, 0] = 2;
        term2[49, 1] = 1;
        term2[49, 2] = -1;
        term2[49, 3] = -1;
        term2[50, 0] = 1;
        term2[50, 2] = 1;
        term2[50, 3] = 1;
        term2[51, 0] = 2;
        term2[51, 1] = -1;
        term2[51, 2] = -2;
        term2[51, 3] = -1;
        term2[52, 1] = 1;
        term2[52, 2] = 2;
        term2[52, 3] = 1;
        term2[53, 0] = 4;
        term2[53, 2] = -2;
        term2[53, 3] = -1;
        term2[54, 0] = 4;
        term2[54, 1] = -1;
        term2[54, 2] = -1;
        term2[54, 3] = -1;
        term2[55, 0] = 1;
        term2[55, 2] = 1;
        term2[55, 3] = -1;
        term2[56, 0] = 4;
        term2[56, 2] = 1;
        term2[56, 3] = -1;
        term2[57, 0] = 1;
        term2[57, 2] = -1;
        term2[57, 3] = -1;
        term2[58, 0] = 4;
        term2[58, 1] = -1;
        term2[58, 3] = -1;
        term2[59, 0] = 2;
        term2[59, 1] = -2;
        term2[59, 3] = 1;
        #endregion

        #region
        term2[0, 4] = 5128122;
        term2[1, 4] = 280602;
        term2[2, 4] = 277693;
        term2[3, 4] = 173237;
        term2[4, 4] = 55413;
        term2[5, 4] = 46271;
        term2[6, 4] = 32573;
        term2[7, 4] = 17198;
        term2[8, 4] = 9266;
        term2[9, 4] = 8822;
        term2[10, 4] = 8216;
        term2[11, 4] = 4324;
        term2[12, 4] = 4200;
        term2[13, 4] = -3359;
        term2[14, 4] = 2463;
        term2[15, 4] = 2211;
        term2[16, 4] = 2065;
        term2[17, 4] = -1870;
        term2[18, 4] = 1828;
        term2[19, 4] = -1794;
        term2[20, 4] = -1749;
        term2[21, 4] = -1565;
        term2[22, 4] = -1491;
        term2[23, 4] = -1475;
        term2[24, 4] = -1410;
        term2[25, 4] = -1344;
        term2[26, 4] = -1335;
        term2[27, 4] = 1107;
        term2[28, 4] = 1021;
        term2[29, 4] = 833;
        term2[30, 4] = 777;
        term2[31, 4] = 671;
        term2[32, 4] = 607;
        term2[33, 4] = 596;
        term2[34, 4] = 491;
        term2[35, 4] = -451;
        term2[36, 4] = 439;
        term2[37, 4] = 422;
        term2[38, 4] = 421;
        term2[39, 4] = -366;
        term2[40, 4] = -351;
        term2[41, 4] = 331;
        term2[42, 4] = 315;
        term2[43, 4] = 302;
        term2[44, 4] = -283;
        term2[45, 4] = -229;
        term2[46, 4] = 223;
        term2[47, 4] = 223;
        term2[48, 4] = -220;
        term2[49, 4] = -220;
        term2[50, 4] = -185;
        term2[51, 4] = 181;
        term2[52, 4] = -177;
        term2[53, 4] = 176;
        term2[54, 4] = 166;
        term2[55, 4] = -164;
        term2[56, 4] = 132;
        term2[57, 4] = -119;
        term2[58, 4] = 115;
        term2[59, 4] = 107;
        #endregion
        #endregion

    }

    void SphereAssoc()
    {

        if (astroNumber == 1 && spheres.Count < 2)
        {
            DeleteSpheres();
            spheres.Clear();
            for (int a = 0; a < 9; a++)
            {
                spheres.Add(Instantiate(sphere));
            }
        }
        else if (astroNumber == 2 && spheres.Count < 2)
        {
            for (int a = 0; a < 4; a++)
            {
                spheres.Add(Instantiate(sphere));
            }
        }

        if (spheres.Count == 0)
        {
            spheres.Add(Instantiate(sphere));
        }
        if (spheres.Count == 1)
        {
            foreach (GameObject sp in spheres)
            {
                sp.transform.parent = activeAstro.transform;
                sp.transform.position = new Vector3(activeAstro.transform.position.x, activeAstro.transform.position.y,
                     activeAstro.transform.position.z);
            }
        } else
        {
            foreach (GameObject sp1 in spheres)
            {
                sp1.transform.parent = activeAstroList[spheres.IndexOf(sp1)].transform;
                sp1.transform.position = new Vector3(activeAstroList[spheres.IndexOf(sp1)].transform.position.x,
                    activeAstroList[spheres.IndexOf(sp1)].transform.position.y, activeAstroList[spheres.IndexOf(sp1)].transform.position.z);

            }
        }
    }

    void DeleteSpheres()
    {
        foreach (GameObject sp in spheres)
        {
            Destroy(sp);

        }
    }

    void ClosestDec()
    {

        if (tempNum != year.value)
        {

            double tempNode = 1000;

            for (int tempJde = (int)Math.Floor(jde - 3652.5); tempJde < jde + 3652.5; tempJde++)
            {
                double t = ((double)tempJde - 2451545) / 36525;

                double ascNode = Normalise(125.0445550 - 1934.1361849 * t + 0.00020762 * Math.Pow(t, 2) + Math.Pow(t, 3) / 80053 +
                    Math.Pow(t, 4) / 18999000);


                if (ascNode < 2 || ascNode > 358)
                {
                    if (Math.Abs(360 - ascNode) < 2)
                    {
                        if (Math.Abs(360 - tempNode) > Math.Abs(360 - ascNode))
                        {
                            tempNode = ascNode;
                            closestAsc = tempJde;
                        }


                    }
                    else if (Math.Abs(0 - ascNode) < 2)
                    {
                        if (Math.Abs(tempNode) > Math.Abs(ascNode))
                        {
                            tempNode = ascNode;
                            closestAsc = tempJde;
                        }


                    }


                }
                else if (ascNode < 182 && ascNode > 178)
                {
                    if (Math.Abs(180 - tempNode) > Math.Abs(180 - ascNode))
                    {
                        tempNode = ascNode;
                        closestDes = tempJde;
                    }


                }
            }

            tempNum = year.value;
            moonText.text = "Asc year: " + JdeToGreg(closestAsc) + "\nDes year: " + JdeToGreg(closestDes);
        }

    }

    string JdeToGreg(double val)
    {
        double tempVal = val;
        tempVal += 0.5;
        tempVal = Math.Floor(tempVal);

        double a;

        if (tempVal < 2299161)
        {
            a = tempVal;
        } else
        {
            double alpha = Math.Floor((tempVal - 1867216.25) / 36524.25);
            a = tempVal + 1 + alpha - Math.Floor(alpha / 4);
        }

        double b = a + 1524;
        double c = Math.Floor((b - 122.1) / 365.25);
        double d = Math.Floor(365.25 * c);
        double e = Math.Floor((b - d) / 30.6001);

        string dayTempString = (b - d - Math.Floor(30.6001 * e)).ToString();
        double monthTemp = 0;

        if (e < 14)
        {
            monthTemp = e - 1;
        } else
        {
            monthTemp = e - 13;
        }

        string monthTempString = monthTemp.ToString();

        string yearTempString = "";

        if (monthTemp > 2)
        {
            yearTempString = (c - 4716).ToString();
        }
        else
        {
            yearTempString = (c - 4715).ToString();
        }

        return dayTempString + "/" + monthTempString + "/" + yearTempString;
    }

    #endregion

    //Star Calculations
    #region

    void CreateAstroData()
    {
        //Alcyone
        astroData.Add(new MasterList(1, 56.87115, 24.10514, 0.000005372222, -0.00001213056));

        //Atlas
        astroData.Add(new MasterList(1, 57.29059, 24.05342, 0.000004916667, -0.00001227222));

        //Electra
        astroData.Add(new MasterList(1, 56.2189, 24.11334, 0.000005788889, -0.00001279444));

        //Maia
        astroData.Add(new MasterList(1, 56.2067, 24.36775, 0.000005858333, -0.00001250833));

        //Merope
        astroData.Add(new MasterList(1, 56.58156, 23.94836, 0.000005869444, -0.000012125));

        //Taygeta
        astroData.Add(new MasterList(1, 56.30207, 24.46728, 0.0000059, -0.00001126667));

        //Pleione
        astroData.Add(new MasterList(1, 57.29673, 24.13671, 0.000005197222, -0.00001298333));

        //Celaeno
        astroData.Add(new MasterList(1, 56.2009, 24.28947, 0.00001168694, -0.00001222222));

        //Sterope
        astroData.Add(new MasterList(1, 56.47699, 24.55451, 0.0000055625, -0.00001276361));

        //Alpha Crux
        astroData.Add(new MasterList(2, 186.6496, -63.09909, -0.000009952778, -0.000004127778));

        //Beta Crux
        astroData.Add(new MasterList(2, 191.9303, -59.68877, -0.00001193611, -0.000004494444));

        //Gamma Crux
        astroData.Add(new MasterList(2, 187.7915, -57.11321, 0.000007841667, -0.00007363333));

        //Delta Crux
        astroData.Add(new MasterList(2, 183.7863, -58.74893, -0.000009947222, -0.000002877778));

        //Sirius
        astroData.Add(new MasterList(3, 101.2872, -16.71612, -0.0001516694, -0.0003397417));

        //Canopus
        astroData.Add(new MasterList(4, 95.98795, -52.69566, 0.000005536111, 0.000006455556));

        //Arcturus
        astroData.Add(new MasterList(5, 213.9154, 19.18222, -0.0003037361, -0.0005553889));

        //Vega
        astroData.Add(new MasterList(6, 279.2347, 38.78369, 0.00005581667, 0.00007950833));

        //Rigel
        astroData.Add(new MasterList(7, 78.63447, -8.201638, 0.000000363888, 0.000000138889));

        //Procyon
        astroData.Add(new MasterList(8, 114.8255, 5.224988, -0.0001984972, -0.000288));

        //Achernar
        astroData.Add(new MasterList(9, 24.42852, -57.23675, 0.00002416667, -0.00001062222));

        //Betelgeuse
        astroData.Add(new MasterList(10, 88.79294, 7.407064, 0.000007338889, 0.000002666667));

    }

    void CalculateStar()
    {
        double starAscension = 0;
        double starDeclination = 0;
        double starMotAsc = 0;
        double starMotDec = 0;
        foreach (MasterList vec in astroData)
        {
            if (astroNumber > 2)
            {
                if (astroNumber == vec.ID)
                {
                    starAscension = vec.ascen;
                    starDeclination = vec.dec;
                    starMotAsc = vec.motAscen;
                    starMotDec = vec.motDec;
                }
            }

            if (astroNumber > 0 && astroNumber < 3)
            {
                CalculateConst();
                return;
            }
        }

        double t = (jde - 2451545) / 36525;

        starAscension = starAscension + (starMotAsc * t * 100);
        starDeclination = starDeclination + (starMotDec * t * 100);


        double starEta = (2306.2181 * t + 0.30188 * Math.Pow(t, 2) + 0.017998 * Math.Pow(t, 3)) / 3600;
        double starZeta = (2306.2181 * t + 1.09468 * Math.Pow(t, 2) + 0.018203 * Math.Pow(t, 3)) / 3600;
        double starTheta = (2004.3109 * t - 0.42665 * Math.Pow(t, 2) - 0.041833 * Math.Pow(t, 3)) / 3600;

        double a = Math.Cos(Mathf.Deg2Rad * starDeclination) * Math.Sin(Mathf.Deg2Rad * (starAscension + starEta));

        double b = Math.Cos(Mathf.Deg2Rad * starTheta) * Math.Cos(Mathf.Deg2Rad * starDeclination) *
            Math.Cos(Mathf.Deg2Rad * (starAscension + starEta)) - Math.Sin(Mathf.Deg2Rad * starTheta) * Math.Sin(Mathf.Deg2Rad * starDeclination);

        double c = Math.Sin(Mathf.Deg2Rad * starTheta) * Math.Cos(Mathf.Deg2Rad * starDeclination) *
            Math.Cos(Mathf.Deg2Rad * (starAscension + starEta)) + Math.Cos(Mathf.Deg2Rad * starTheta) * Math.Sin(Mathf.Deg2Rad * starDeclination);

        starAscension = Mathf.Rad2Deg * Math.Atan2(a, b) + starZeta;
        starDeclination = Mathf.Rad2Deg * Math.Asin(c);


        double apparentAsc = starAscension + deltaPsi * (Math.Cos(Mathf.Deg2Rad * trueAxis) + Math.Sin(Mathf.Deg2Rad * trueAxis) *
            Math.Sin(Mathf.Deg2Rad * starAscension) * Math.Tan(Mathf.Deg2Rad * starDeclination)) - deltaEpsilon *
            (Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Tan(Mathf.Deg2Rad * starDeclination));

        double apparentDec = starDeclination + deltaPsi * (Math.Sin(Mathf.Deg2Rad * trueAxis) * Math.Cos(Mathf.Deg2Rad * starAscension)) +
            deltaEpsilon * (Math.Sin(Mathf.Deg2Rad * starAscension));

        double tempMeanLongitude = Normalise(280.46645 + 36000.76983 * t + 0.0003032 * Math.Pow(t, 2));
        double meanAnomaly = Normalise(357.52910 + 35999.05030 * t -
            0.0001559 * Math.Pow(t, 2) - 0.00000048 * Math.Pow(t, 3));
        double sunCentre = (1.914600 - 0.004817 * t - 0.000014 * Math.Pow(t, 2)) * Math.Sin(Mathf.Deg2Rad * meanAnomaly) +
            (0.019993 - 0.000101 * t) * Math.Sin(Mathf.Deg2Rad * (2 * meanAnomaly)) +
            0.000290 * Math.Sin(Mathf.Deg2Rad * (3 * meanAnomaly));

        double trueGeoLong = tempMeanLongitude + sunCentre;

        double k = 0.0056932;

        double e = 0.016708617 - 0.000042037 * t - 0.0000001236 * Math.Pow(t, 2);

        double pi = 102.93735 + 1.71953 * t + 0.00046 * Math.Pow(t, 2);

        apparentAsc = apparentAsc - k * ((Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Cos(Mathf.Deg2Rad * trueGeoLong) *
            Math.Cos(Mathf.Deg2Rad * trueAxis) + Math.Sin(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * trueGeoLong)) /
            Math.Cos(Mathf.Deg2Rad * starDeclination)) + e * k * ((Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Cos(Mathf.Deg2Rad * pi) *
            Math.Cos(Mathf.Deg2Rad * trueAxis) + Math.Sin(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * pi)) /
            Math.Cos(Mathf.Deg2Rad * starDeclination));

        apparentDec = apparentDec - k * (Math.Cos(Mathf.Deg2Rad * trueGeoLong) * Math.Cos(Mathf.Deg2Rad * trueAxis) *
            (Math.Tan(Mathf.Deg2Rad * trueAxis) * Math.Cos(Mathf.Deg2Rad * starDeclination) - Math.Sin(Mathf.Deg2Rad * starAscension) *
            Math.Sin(Mathf.Deg2Rad * starDeclination)) + Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * starDeclination) *
            Math.Sin(Mathf.Deg2Rad * trueGeoLong)) + e * k * (Math.Cos(Mathf.Deg2Rad * pi) * Math.Cos(Mathf.Deg2Rad * trueAxis) *
            (Math.Tan(Mathf.Deg2Rad * trueAxis) * Math.Cos(Mathf.Deg2Rad * starDeclination) - Math.Sin(Mathf.Deg2Rad * starAscension) *
            Math.Sin(Mathf.Deg2Rad * starDeclination)) + Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * starDeclination) *
            Math.Sin(Mathf.Deg2Rad * pi));

        starAscension = apparentAsc;
        starDeclination = apparentDec;

        if (starDeclination < 0)
        {
            starDeclination = starDeclination + 360;
        }
        if (starAscension < 0) {
            starAscension += 360;
        }

        double greenTime = Normalise(280.46061837 + 360.98564736629 * (jde - 2451545) + 0.000387933 * Math.Pow(t, 2) -
            Math.Pow(t, 3) / 38710000 + deltaPsi * Math.Cos(Mathf.Deg2Rad * trueAxis));

        double hourAngle = Normalise(greenTime - longitude - starAscension);

        if (hourAngle < 0)
        {
            hourAngle += 360;
        }

        double starAzimuth = Mathf.Rad2Deg * Math.Atan2(Math.Sin(Mathf.Deg2Rad * hourAngle), Math.Cos(Mathf.Deg2Rad * hourAngle) *
            Math.Sin(Mathf.Deg2Rad * latitude) - Math.Tan(Mathf.Deg2Rad * starDeclination) * Math.Cos(Mathf.Deg2Rad * latitude));

        double starAltitude = Mathf.Rad2Deg * Math.Asin(Math.Sin(Mathf.Deg2Rad * latitude) * Math.Sin(Mathf.Deg2Rad * starDeclination) +
            Math.Cos(Mathf.Deg2Rad * latitude) * Math.Cos(Mathf.Deg2Rad * starDeclination) * Math.Cos(Mathf.Deg2Rad * hourAngle));

        if (starAzimuth < 0)
        {
            starAzimuth += 360;
        }

        double tempAz = starAzimuth + 180;

        starAzimuth = -(tempAz + 90);


        if (starAzimuth < 0)
        {
            starAzimuth += 360;
        }

        double earthToStar = 5000;
        scalingFactor = 0;

        absoluteAltitude = starAltitude;
        absoluteDistance = earthToStar;

        if (refraction == true)
        {
            altitude = CalculateRefraction(starAltitude);
        }

        Vector3 pos = SphericalToCartesian(earthToStar, starAzimuth, starAltitude);

        activeAstro.transform.position = pos;
    }

    void CalculateConst()
    {
        List<MasterList> constList = new List<MasterList>();

        foreach (MasterList mas in astroData)
        {
            if (astroNumber == 1)
            {
                if (mas.ID == 1)
                {
                    constList.Add(mas);
                }

            } else if (astroNumber == 2)
            {
                if (mas.ID == 2)
                {
                    constList.Add(mas);
                }
            }
        }

        foreach (MasterList m in constList)
        {

            double starAscension = m.ascen;
            double starDeclination = m.dec;
            double starMotAsc = m.motAscen;
            double starMotDec = m.motDec;

            double t = (jde - 2451545) / 36525;

            starAscension = starAscension + (starMotAsc * t * 100);
            starDeclination = starDeclination + (starMotDec * t * 100);

            double starEta = (2306.2181 * t + 0.30188 * Math.Pow(t, 2) + 0.017998 * Math.Pow(t, 3)) / 3600;
            double starZeta = (2306.2181 * t + 1.09468 * Math.Pow(t, 2) + 0.018203 * Math.Pow(t, 3)) / 3600;
            double starTheta = (2004.3109 * t - 0.42665 * Math.Pow(t, 2) - 0.041833 * Math.Pow(t, 3)) / 3600;

            double a = Math.Cos(Mathf.Deg2Rad * starDeclination) * Math.Sin(Mathf.Deg2Rad * (starAscension + starEta));

            double b = Math.Cos(Mathf.Deg2Rad * starTheta) * Math.Cos(Mathf.Deg2Rad * starDeclination) *
                Math.Cos(Mathf.Deg2Rad * (starAscension + starEta)) - Math.Sin(Mathf.Deg2Rad * starTheta) * Math.Sin(Mathf.Deg2Rad * starDeclination);

            double c = Math.Sin(Mathf.Deg2Rad * starTheta) * Math.Cos(Mathf.Deg2Rad * starDeclination) *
                Math.Cos(Mathf.Deg2Rad * (starAscension + starEta)) + Math.Cos(Mathf.Deg2Rad * starTheta) * Math.Sin(Mathf.Deg2Rad * starDeclination);

            starAscension = Mathf.Rad2Deg * Math.Atan2(a, b) + starZeta;
            starDeclination = Mathf.Rad2Deg * Math.Asin(c);

            double apparentAsc = starAscension + deltaPsi * (Math.Cos(Mathf.Deg2Rad * trueAxis) + Math.Sin(Mathf.Deg2Rad * trueAxis) *
                Math.Sin(Mathf.Deg2Rad * starAscension) * Math.Tan(Mathf.Deg2Rad * starDeclination)) - deltaEpsilon *
                (Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Tan(Mathf.Deg2Rad * starDeclination));

            double apparentDec = starDeclination + deltaPsi * (Math.Sin(Mathf.Deg2Rad * trueAxis) * Math.Cos(Mathf.Deg2Rad * starAscension)) +
                deltaEpsilon * (Math.Sin(Mathf.Deg2Rad * starAscension));

            double tempMeanLongitude = Normalise(280.46645 + 36000.76983 * t + 0.0003032 * Math.Pow(t, 2));
            double meanAnomaly = Normalise(357.52910 + 35999.05030 * t -
                0.0001559 * Math.Pow(t, 2) - 0.00000048 * Math.Pow(t, 3));
            double sunCentre = (1.914600 - 0.004817 * t - 0.000014 * Math.Pow(t, 2)) * Math.Sin(Mathf.Deg2Rad * meanAnomaly) +
                (0.019993 - 0.000101 * t) * Math.Sin(Mathf.Deg2Rad * (2 * meanAnomaly)) +
                0.000290 * Math.Sin(Mathf.Deg2Rad * (3 * meanAnomaly));

            double trueGeoLong = tempMeanLongitude + sunCentre;

            double k = 0.0056932;

            double e = 0.016708617 - 0.000042037 * t - 0.0000001236 * Math.Pow(t, 2);

            double pi = 102.93735 + 1.71953 * t + 0.00046 * Math.Pow(t, 2);

            apparentAsc = apparentAsc - k * ((Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Cos(Mathf.Deg2Rad * trueGeoLong) *
            Math.Cos(Mathf.Deg2Rad * trueAxis) + Math.Sin(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * trueGeoLong)) /
            Math.Cos(Mathf.Deg2Rad * starDeclination)) + e * k * ((Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Cos(Mathf.Deg2Rad * pi) *
            Math.Cos(Mathf.Deg2Rad * trueAxis) + Math.Sin(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * pi)) /
            Math.Cos(Mathf.Deg2Rad * starDeclination));

            apparentDec = apparentDec - k * (Math.Cos(Mathf.Deg2Rad * trueGeoLong) * Math.Cos(Mathf.Deg2Rad * trueAxis) *
                (Math.Tan(Mathf.Deg2Rad * trueAxis) * Math.Cos(Mathf.Deg2Rad * starDeclination) - Math.Sin(Mathf.Deg2Rad * starAscension) *
                Math.Sin(Mathf.Deg2Rad * starDeclination)) + Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * starDeclination) *
                Math.Sin(Mathf.Deg2Rad * trueGeoLong)) + e * k * (Math.Cos(Mathf.Deg2Rad * pi) * Math.Cos(Mathf.Deg2Rad * trueAxis) *
                (Math.Tan(Mathf.Deg2Rad * trueAxis) * Math.Cos(Mathf.Deg2Rad * starDeclination) - Math.Sin(Mathf.Deg2Rad * starAscension) *
                Math.Sin(Mathf.Deg2Rad * starDeclination)) + Math.Cos(Mathf.Deg2Rad * starAscension) * Math.Sin(Mathf.Deg2Rad * starDeclination) *
                Math.Sin(Mathf.Deg2Rad * pi));

            starAscension = apparentAsc;
            starDeclination = apparentDec;

            if (starDeclination < 0)
            {
                starDeclination = starDeclination + 360;
            }
            if (starAscension < 0)
            {
                starAscension += 360;
            }

            double greenTime = Normalise(280.46061837 + 360.98564736629 * (jde - 2451545) + 0.000387933 * Math.Pow(t, 2) -
                Math.Pow(t, 3) / 38710000 + deltaPsi * Math.Cos(Mathf.Deg2Rad * trueAxis));

            double hourAngle = Normalise(greenTime - longitude - starAscension);

            if (hourAngle < 0)
            {
                hourAngle += 360;
            }

            double starAzimuth = Mathf.Rad2Deg * Math.Atan2(Math.Sin(Mathf.Deg2Rad * hourAngle), Math.Cos(Mathf.Deg2Rad * hourAngle) *
                Math.Sin(Mathf.Deg2Rad * latitude) - Math.Tan(Mathf.Deg2Rad * starDeclination) * Math.Cos(Mathf.Deg2Rad * latitude));

            double starAltitude = Mathf.Rad2Deg * Math.Asin(Math.Sin(Mathf.Deg2Rad * latitude) * Math.Sin(Mathf.Deg2Rad * starDeclination) +
                Math.Cos(Mathf.Deg2Rad * latitude) * Math.Cos(Mathf.Deg2Rad * starDeclination) * Math.Cos(Mathf.Deg2Rad * hourAngle));

            if (starAzimuth < 0)
            {
                starAzimuth += 360;
            }

            double tempAz = starAzimuth + 180;

            starAzimuth = -(tempAz + 90);


            if (starAzimuth < 0)
            {
                starAzimuth += 360;
            }
            double earthToStar = 50000;
            scalingFactor = 0;

            if (savCon == true)
            {
                double[] tempDob = new double[2];

                tempDob[0] = starAltitude;
                tempDob[1] = earthToStar;

                absolutes.Add(tempDob);
            }

            if (refraction == true)
            {
                altitude = CalculateRefraction(starAltitude);
            }

            Vector3 pos = SphericalToCartesian(earthToStar, starAzimuth, starAltitude);


            activeAstroList[constList.IndexOf(m)].transform.position = pos;
        }


    }

    #endregion

    //Saving
    #region

    void ButtonPressed()
    {
        rowData.Clear();
        jdeList.Clear();

        double firstAlig = 0;
        double firstAligMon = 0, firstAligDay = 0, firstAligHour = 0, firstAligMin = 0,
            lastAligMon = 0, lastAligDay = 0, lastAligHour = 0, lastAligMin = 0, startOrientation = 0, endOrientation = 1;

        string[] rowDataTemp = new string[12];
        rowDataTemp[0] = "Astro";
        rowDataTemp[1] = "Year";
        rowDataTemp[2] = "Start month";
        rowDataTemp[3] = "Start day";
        rowDataTemp[4] = "Start hour";
        rowDataTemp[5] = "Start minute";
        rowDataTemp[6] = "Start orientation";
        rowDataTemp[7] = "End month";
        rowDataTemp[8] = "End day";
        rowDataTemp[9] = "End hour";
        rowDataTemp[10] = "End minute";
        rowDataTemp[11] = "End orientation";
        rowData.Add(rowDataTemp);

        if (activeAstroList.Count == 0)
        {
            for (int a = 1; a < 13; a++)
            {
                int days;

                if (a == 1 || a == 3 || a == 5 || a == 7 || a == 8 || a == 10 || a == 12)
                {
                    days = 32;
                }
                else if (a == 2)
                {
                    days = 29;
                }
                else
                {
                    days = 31;
                }

                for (int b = 1; b < days; b++)
                {
                    for (int c = 0; c < 24; c++)
                    {
                        for (int d = 0; d < 60; d++)
                        {

                            month.value = a;
                            day.value = b;
                            hour.value = c;
                            minute.value = d;

                            Calculations();

                            double hor = CalculateTargetHorizon(absoluteAltitude, absoluteDistance);

                            if (Physics.Raycast(activeAstro.transform.position,
                                target.transform.position - activeAstro.transform.position, out RaycastHit hit,
                                Vector3.Distance(activeAstro.transform.position, target.transform.position)))
                            {

                                if (hit.collider.tag == "Target")
                                {

                                    if (firstAlig == 0)
                                    {
                                        if (hor > 0)
                                        {
                                            firstAlig = 1;
                                            firstAligMon = a;
                                            firstAligDay = b;
                                            firstAligHour = c;
                                            firstAligMin = d;
                                            startOrientation = (Math.Atan2(-activeAstro.transform.position.x, -activeAstro.transform.position.z)
                                                * Mathf.Rad2Deg);

                                            jdeList.Add(jde);
                                        }
                                    }
                                    else if (firstAlig != 0 && hor < 0)
                                    {
                                        lastAligMon = a;
                                        lastAligDay = b;
                                        lastAligHour = c;
                                        lastAligMin = d;
                                        endOrientation = (Math.Atan2(-activeAstro.transform.position.x, -activeAstro.transform.position.z)
                                            * Mathf.Rad2Deg);

                                        SaveFile(activeAstro.name, firstAligMon, firstAligDay, firstAligHour, firstAligMin,
                                                lastAligMon, lastAligDay, lastAligHour, lastAligMin, startOrientation, endOrientation);

                                        firstAlig = 0;
                                    }
                                }
                                else if (firstAlig != 0)
                                {
                                    lastAligMon = a;
                                    lastAligDay = b;
                                    lastAligHour = c;
                                    lastAligMin = d;
                                    endOrientation = (Math.Atan2(-activeAstro.transform.position.x, -activeAstro.transform.position.z)
                                                 * Mathf.Rad2Deg);


                                    SaveFile(activeAstro.name, firstAligMon, firstAligDay, firstAligHour, firstAligMin,
                                            lastAligMon, lastAligDay, lastAligHour, lastAligMin, startOrientation, endOrientation);

                                    firstAlig = 0;
                                }
                            }
                        }
                    }
                }
            }
        } else
        {
            int absCount = 0;
            int tempInd = activeAstroList.Count;
            int[] astrNum = new int[tempInd];
            for (int v = 0; v < tempInd; v++)
            {
                astrNum[v] = 0;
            }
            absolutes.Clear();
            for (int z = 0; z < activeAstroList.Count; z++)
            {

                GameObject astr = activeAstroList[z];
                savCon = true;
                for (int a = 1; a < 13; a++)
                {
                    int days;

                    if (a == 1 || a == 3 || a == 5 || a == 7 || a == 8 || a == 10 || a == 12)
                    {
                        days = 32;
                    }
                    else if (a == 2)
                    {
                        days = 29;
                    }
                    else
                    {
                        days = 31;
                    }

                    for (int b = 1; b < days; b++)
                    {
                        for (int c = 0; c < 24; c++)
                        {
                            for (int d = 0; d < 60; d++)
                            {
                                month.value = a;
                                day.value = b;
                                hour.value = c;
                                minute.value = d;
                                Calculations();
                                double hor = CalculateTargetHorizon(absolutes[absCount][0], absolutes[absCount][1]);
                                absCount++;


                                if (Physics.Raycast(astr.transform.position,
                                target.transform.position - astr.transform.position, out RaycastHit hit,
                                Vector3.Distance(astr.transform.position, target.transform.position)))
                                {
                                    if (hit.collider.tag == "Target")
                                    {
                                        if (astrNum[activeAstroList.IndexOf(astr)] == 0)
                                        {
                                            if (hor > 0)
                                            {
                                                astrNum[activeAstroList.IndexOf(astr)] = 1;
                                                firstAligMon = a;
                                                firstAligDay = b;
                                                firstAligHour = c;
                                                firstAligMin = d;
                                                startOrientation = (Math.Atan2(-astr.transform.position.x, -astr.transform.position.z)
                                                * Mathf.Rad2Deg);

                                            }
                                        }
                                        else if (astrNum[activeAstroList.IndexOf(astr)] != 0 && hor < 0)
                                        {
                                            lastAligMon = a;
                                            lastAligDay = b;
                                            lastAligHour = c;
                                            lastAligMin = d;
                                            endOrientation = (Math.Atan2(-astr.transform.position.x, -astr.transform.position.z)
                                                * Mathf.Rad2Deg);


                                            SaveFile(astr.name, firstAligMon, firstAligDay, firstAligHour, firstAligMin,
                                                    lastAligMon, lastAligDay, lastAligHour, lastAligMin, startOrientation, endOrientation);

                                            astrNum[activeAstroList.IndexOf(astr)] = 0;
                                        }
                                    }
                                    else if (astrNum[activeAstroList.IndexOf(astr)] != 0)
                                    {
                                        lastAligMon = a;
                                        lastAligDay = b;
                                        lastAligHour = c;
                                        lastAligMin = d;
                                        endOrientation = (Math.Atan2(-astr.transform.position.x, -astr.transform.position.z)
                                                 * Mathf.Rad2Deg);


                                        SaveFile(astr.name, firstAligMon, firstAligDay, firstAligHour, firstAligMin,
                                                lastAligMon, lastAligDay, lastAligHour, lastAligMin, startOrientation, endOrientation);

                                        astrNum[activeAstroList.IndexOf(astr)] = 0;
                                    }
                                }
                            }
                        }
                    }
                }
                savCon = false;
            }
        }

        AddExtras();
        SaveDocument();
    }

    void SaveFile(String name, Double firstAligMon, Double firstAligDay, Double firstAligHour, Double firstAligMin,
            Double lastAligMon, Double lastAligDay, Double lastAligHour, Double lastAligMin, Double startOri, Double endOri)
    {
        string[] rowDataTemp = new string[12];

        rowDataTemp[0] = name;
        rowDataTemp[1] = yearText.text;
        rowDataTemp[2] = firstAligMon.ToString();
        rowDataTemp[3] = firstAligDay.ToString();
        rowDataTemp[4] = firstAligHour.ToString();
        rowDataTemp[5] = firstAligMin.ToString();
        rowDataTemp[6] = startOri.ToString();
        rowDataTemp[7] = lastAligMon.ToString();
        rowDataTemp[8] = lastAligDay.ToString();
        rowDataTemp[9] = lastAligHour.ToString();
        rowDataTemp[10] = lastAligMin.ToString();
        rowDataTemp[11] = endOri.ToString();
        rowData.Add(rowDataTemp);

    }

    void SaveDocument()
    {
        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        string tempName = "";
        if (astroChoice.value != 2 && astroChoice.value != 3)
        {
            tempName = activeAstro.name.ToString();

        } else
        {
            if (astroChoice.value == 2)
            {
                tempName = "Pleiades";
            }
            if (astroChoice.value == 3)
            {
                tempName = "Crux";
            }
        }

        string tempYear = "";
        if (year.value < 2000)
        {
            tempYear = ((2000 - year.value).ToString()) + "_AD";
        } else
        {
            tempYear = ((year.value - 2000).ToString()) + "_BC";
        }

        string filePath = Application.dataPath + "/CSV/" + tempName + "_" + tempYear + "_" + siteName + ".csv";

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    void AddExtras()
    {
        if (activeAstro == sun)
        {
            string[] rowDataTemp = new string[10];
            rowDataTemp[0] = "Equinoxes/Solstices:";
            rowDataTemp[1] = "March equinox: ";
            rowDataTemp[2] = dateSprEq;
            rowDataTemp[3] = "Summer solstice: ";
            rowDataTemp[4] = dateSumSol;
            rowDataTemp[5] = "September equinox: ";
            rowDataTemp[6] = dateAutEq;
            rowDataTemp[7] = "Winter solstice: ";
            rowDataTemp[8] = dateWinSol;
            rowData.Add(rowDataTemp);

            for (int a = 1; a < jdeList.Count; a++)
            {
                double tempJde = jdeList[a - 1];

                if (Math.Floor(tempJde) == jdeSprEq)
                {
                    jde = tempJde;
                    CalculateDay();
                    CalculateEarthAxis();
                    SiteHeight();
                    ParseFields();
                    SunCalculations();
                    oriSprEq = (Math.Atan2(-sun.transform.position.x, -sun.transform.position.z) * Mathf.Rad2Deg + 180).ToString();
                }

                if (Math.Floor(tempJde) == jdeSumSol)
                {
                    jde = tempJde;
                    CalculateDay();
                    CalculateEarthAxis();
                    SiteHeight();
                    ParseFields();
                    SunCalculations();
                    oriSumSol = (Math.Atan2(-sun.transform.position.x, -sun.transform.position.z) * Mathf.Rad2Deg + 180).ToString();
                }

                if (Math.Floor(tempJde) == jdeAutEq)
                {
                    jde = tempJde;
                    CalculateDay();
                    CalculateEarthAxis();
                    SiteHeight();
                    ParseFields();
                    SunCalculations();
                    oriAutEq = (Math.Atan2(-sun.transform.position.x, -sun.transform.position.z) * Mathf.Rad2Deg + 180).ToString();
                }

                if (Math.Floor(tempJde) == jdeWinSol)
                {
                    jde = tempJde;
                    CalculateDay();
                    CalculateEarthAxis();
                    SiteHeight();
                    ParseFields();
                    SunCalculations();
                    oriWinSol = (Math.Atan2(-sun.transform.position.x, -sun.transform.position.z) * Mathf.Rad2Deg + 180).ToString();
                }

            }


            rowDataTemp = new string[10];
            rowDataTemp[0] = "Ideal orientations:";
            rowDataTemp[1] = "March equinox: ";
            rowDataTemp[2] = oriSprEq;
            rowDataTemp[3] = "Summer solstice: ";
            rowDataTemp[4] = oriSumSol;
            rowDataTemp[5] = "September equinox: ";
            rowDataTemp[6] = oriAutEq;
            rowDataTemp[7] = "Winter solstice: ";
            rowDataTemp[8] = oriWinSol;
            rowData.Add(rowDataTemp);

            rowDataTemp = new string[10];
            rowDataTemp[0] = "Site orientation:";
            rowDataTemp[1] = oriField.text;
            rowData.Add(rowDataTemp);


        }

        if (activeAstro == moon)
        {
            string[] rowDataTemp = new string[10];
            rowDataTemp[0] = "Ascending/Descending: ";
            rowDataTemp[1] = "Ascending date: ";
            rowDataTemp[2] = JdeToGreg(closestAsc);
            rowDataTemp[3] = "Descending date: ";
            rowDataTemp[4] = JdeToGreg(closestDes);
            rowData.Add(rowDataTemp);

            for (int a = 1; a < jdeList.Count; a++)
            {
                double tempJde = jdeList[a - 1];

                if (Math.Floor(tempJde) == closestAsc)
                {
                    MoonCalculations();
                    ascOri = Math.Atan2(-sun.transform.position.x, -sun.transform.position.z) * Mathf.Rad2Deg + 180;
                }

                if (Math.Floor(tempJde) == closestDes)
                {
                    MoonCalculations();
                    desOri = Math.Atan2(-sun.transform.position.x, -sun.transform.position.z) * Mathf.Rad2Deg + 180;
                }
            }

            rowDataTemp = new string[10];
            rowDataTemp[0] = "Orientations: ";
            rowDataTemp[1] = "Ascending ori: ";
            rowDataTemp[2] = ascOri.ToString(); ;
            rowDataTemp[3] = "Descending ori: ";
            rowDataTemp[4] = desOri.ToString();
            rowData.Add(rowDataTemp);

            rowDataTemp = new string[10];
            rowDataTemp[0] = "Site orientation:";
            rowDataTemp[1] = oriField.text;
            rowData.Add(rowDataTemp);
        }


    }

    double CalculateTargetHorizon(double absoluteAltitude, double absoluteDistance)
    {
        double z = Math.Atan2(target.transform.position.y, absoluteDistance);
        double y = 90 - z;


        return absoluteAltitude - targetHorizon + y;
    }

    void QuickSave()
    {

        /*if (Input.GetKeyDown(KeyCode.Return))
        {
            for (int b = 0; b < 12; b++)
            {
                year.value = 5600;
                astroChoice.value = b;
                ChooseAstro();
                AssignSiteHeight();

                if (b != 1)
                {
                    for (int a = 0; a < 6; a++)
                    {
                        string tempYear = "";

                        if (year.value < 2000)
                        {
                            tempYear = (2000 - year.value).ToString() + " AD";
                        }
                        else
                        {
                            tempYear = (year.value - 2000).ToString() + " BC";
                        }

                        yearText.text = "Year: " + tempYear;
                        ButtonPressed();
                        year.value -= 300;
                    }
                } else
                {
                    year.value = 5606;
                    YearDecide();
                    ButtonPressed();
                    year.value = 5597;
                    YearDecide();
                    ButtonPressed();
                    year.value = 5299;
                    YearDecide();
                    ButtonPressed();
                    year.value = 5290;
                    YearDecide();
                    ButtonPressed();
                    year.value = 5001;
                    YearDecide();
                    ButtonPressed();
                    year.value = 4992;
                    YearDecide();
                    ButtonPressed();
                    year.value = 4703;
                    YearDecide();
                    ButtonPressed();
                    year.value = 4694;
                    YearDecide();
                    ButtonPressed();
                    year.value = 4405;
                    YearDecide();
                    ButtonPressed();
                    year.value = 4396;
                    YearDecide();
                    ButtonPressed();
                    year.value = 4108;
                    YearDecide();
                    ButtonPressed();
                    year.value = 4098;
                    YearDecide();
                    ButtonPressed();
                }
            }
        }*/


        if (Input.GetKeyDown(KeyCode.Return))
        {
            year.value = 5600;
            AssignSiteHeight();

            if (astroChoice.value != 1)
            {
                for (int a = 0; a < 6; a++)
                {
                    string tempYear = "";

                    if (year.value < 2000)
                    {
                        tempYear = (2000 - year.value).ToString() + " AD";
                    }
                    else
                    {
                        tempYear = (year.value - 2000).ToString() + " BC";
                    }

                    yearText.text = "Year: " + tempYear;
                    ButtonPressed();
                    year.value -= 300;
                }
            } else
            {
                year.value = 5606;
                YearDecide();
                ButtonPressed();
                year.value = 5597;
                YearDecide();
                ButtonPressed();
                year.value = 5299;
                YearDecide();
                ButtonPressed();
                year.value = 5290;
                YearDecide();
                ButtonPressed();
                year.value = 5001;
                YearDecide();
                ButtonPressed();
                year.value = 4992;
                YearDecide();
                ButtonPressed();
                year.value = 4703;
                YearDecide();
                ButtonPressed();
                year.value = 4694;
                YearDecide();
                ButtonPressed();
                year.value = 4405;
                YearDecide();
                ButtonPressed();
                year.value = 4396;
                YearDecide();
                ButtonPressed();
                year.value = 4108;
                YearDecide();
                ButtonPressed();
                year.value = 4098;
                YearDecide();
                ButtonPressed();
            }
        }
    }

    void YearDecide()
    {
        string tempYear = "";

        if (year.value < 2000)
        {
            tempYear = (2000 - year.value).ToString() + " AD";
        }
        else
        {
            tempYear = (year.value - 2000).ToString() + " BC";
        }

        yearText.text = "Year: " + tempYear;
    }


    #endregion
}

//MasterList class
#region

[System.Serializable]
public class MasterList
{

    public double ID, ascen, dec, motAscen, motDec;

    public MasterList(Double a, Double b, Double c, Double d, Double e)
    {
        this.ID = a;
        this.ascen = b;
        this.dec = c;
        this.motAscen = d;
        this.motDec = e;
    }
}

#endregion