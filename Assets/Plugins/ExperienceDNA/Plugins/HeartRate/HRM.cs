using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HRM : MonoBehaviour {

    public int Bpm { get; private set; }
    public string DevicePtrn = "Polar";
    public string ServicePtrn = "180d";
    public string CharactPtrn = "2a37";
    public Text StatusText;

    private bool _isScanningDevices = false;
    private bool _isScanningServices = false;
    private bool _isScanningCharacteristics = false;
    private bool _isSubscribed = false;

    private string _selectedDeviceId;
    private string _selectedServiceId;
    private string _selectedCharacteristicId;

    
    private static float TIMEOUT = 6f;
    private List<int> _hri;

    private void Start() {
        _hri = new List<int>();
    }

    public void Connect(Text statusText = null) {
        this.StatusText = statusText;
        StartCoroutine(AutoConnectSequence(DevicePtrn, ServicePtrn, CharactPtrn));
    }

    private IEnumerator AutoConnectSequence(string devicePtrn, string servicePtrn, string charactPtrn) {
        _selectedDeviceId = null;
        yield return FindDevice(devicePtrn);
        if(_selectedDeviceId == null) {
            SetStatus("Could not find device with pattern " + devicePtrn);
            yield break;
        }

        _selectedServiceId = null;
        yield return FindService(servicePtrn);
        if(_selectedServiceId == null) {
            SetStatus("Could not find appropriate service on device " + _selectedDeviceId);
            yield break;
        }

        _selectedCharacteristicId = null;
        yield return FindCharacteristic(charactPtrn);
        if(_selectedCharacteristicId == null) {
            SetStatus("Could not find appropriate characteristic in service " + _selectedServiceId);
            yield break;
        }


        BleApi.SubscribeCharacteristic(_selectedDeviceId, _selectedServiceId, _selectedCharacteristicId, false);
        SetStatus("Succesfully subscribed HRM: " + _selectedDeviceId + "; " + _selectedServiceId + "; " + _selectedCharacteristicId);
        _isSubscribed = true;
        _hri.Clear();
    }

    private IEnumerator FindDevice(string devicePtrn) {
        BleApi.StopDeviceScan();
        BleApi.StartDeviceScan();
        _isScanningDevices = true;

        BleApi.DeviceUpdate deviceUpdate;
        float timeout = 0f;
        SetStatus("Searching device: " + devicePtrn);

        while(_isScanningDevices & timeout < TIMEOUT) {

            deviceUpdate = new BleApi.DeviceUpdate();

            if(BleApi.PollDevice(ref deviceUpdate, false) == BleApi.ScanStatus.AVAILABLE) {
                if(deviceUpdate.name.Contains(devicePtrn)) {
                    _selectedDeviceId = deviceUpdate.id;
                    _isScanningDevices = false;
                }
            }

            timeout += Time.deltaTime;
            yield return null;
        }

        BleApi.StopDeviceScan();
    }

    private IEnumerator FindService(string servicePtrn) {
        BleApi.ScanServices(_selectedDeviceId);
        _isScanningServices = true;

        BleApi.Service service = default;
        float timeout = 0f;
        SetStatus("Searching service: " + servicePtrn);

        while(_isScanningServices & timeout < TIMEOUT) {

            if(BleApi.PollService(out service, false) == BleApi.ScanStatus.AVAILABLE) {
                if(service.uuid.Contains(servicePtrn)) {
                    _selectedServiceId = service.uuid;
                    _isScanningServices = false;
                }
            }

            timeout += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FindCharacteristic(string charactPtrn) {
        BleApi.ScanCharacteristics(_selectedDeviceId, _selectedServiceId);
        _isScanningCharacteristics = true;

        BleApi.Characteristic characteristic;
        float timeout = 0f;
        SetStatus("Searching service: " + charactPtrn);

        while(_isScanningCharacteristics & timeout < TIMEOUT) {
            
            if(BleApi.PollCharacteristic(out characteristic, false) == BleApi.ScanStatus.AVAILABLE) {
                if(characteristic.uuid.Contains(charactPtrn)) {
                    _selectedCharacteristicId = characteristic.uuid;
                    _isScanningCharacteristics = false;
                }
            }

            timeout += Time.deltaTime;
            yield return null;
        }
    }

    private void Update() {
        if (!_isSubscribed)
            return;

        BleApi.BLEData res;
        while (BleApi.PollData(out res, false)) {
            Bpm = res.buf[1];
            for (int i = 2; i < res.size - 1; i = i + 2)
                _hri.Add(res.buf[i] + 256 * res.buf[i + 1]);
            SetStatus("BPM: " + Bpm);
        }
    }

    private void SetStatus(string status) {
        if(StatusText == null)
            return;
        StatusText.text = status;
    }

    public List<int> PollHRI() {
        List<int> toReturn = _hri;
        _hri = new List<int>();
        return toReturn;
    }
}
