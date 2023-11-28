# Data Enrichment Toolchain by Amper
## Introduction
### Description
This repository contains the DET (Data Enrichment Toolchain) components developed by Amper for SALTED project, within the SALTED project. This includes data collectors, curator, linkers and enrichers proccess.
### Workflow
The DET start reading data from geo-json files that contain data for SIGPAC Crop parcels and for CO2 footprint data refered to the same geolocation. This data is mapped to smart data model entities and publishing in an NGSI-LD Context Broker. Below are briefly explained the data flow.
  - Data Collector read geo-json files and map the data to NGSI-LD smart data models.
    -   AirQualityObserved for CO2 data.
    -   AgriCrop for the type of Crops.
    -   AgriParcel for the SIGPAC Crop Parcel data.
  - Data Curator assesses the quality of the data.
  - Data Linker and Enricher add value to existing data by adding nwe properties for setting the relationship between AirQualityObserved and AgriParcel entities.

 ### Other components
 N/A
 ### Contact
 All code located in this repository has been developed by Amper.

 ## Installation
 The most simple and effective way of installing Amper DET is by downloading the provided application. Before executig the application, you have to configure your particular access data. you can do that by modifying SALTED_DataInjection.exe.config file.

 ## Acknowledgement
 This work was supported by the EuropeanCommission CEF Programme by means of the project SALTED "Situation Aware Linked heTerogeneous Enrichment Data" under the Action Number 2020-EU-IA-0274.

 ## License
 N/A
 
    
