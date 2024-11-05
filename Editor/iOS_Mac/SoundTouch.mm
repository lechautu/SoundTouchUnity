//
//  SoundTouch.m
//  soundtouch-macos
//
//  Created by Tu Le on 4/11/24.
//

#include "SoundTouch.h"
#include "/Users/tu.le/projects/soundtouch/include/SoundTouch.h"
#include "/Users/tu.le/projects/soundtouch/include/BPMDetect.h"

using namespace soundtouch;

int soundtouch_getVersionId() {
    return SoundTouch::getVersionId();
}

void* soundtouch_createInstance() {
    return new SoundTouch();
}

void soundtouch_destroyInstance(void* st) {
    delete static_cast<SoundTouch*>(st);
}

const char* soundtouch_getVersionString() {
    return SoundTouch::getVersionString();
}

void soundtouch_setRate(void* st, float newRate){
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setRate(newRate);
}

void soundtouch_setTempo(void* st, float newTempo) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setTempo(newTempo);
}

void soundtouch_setRateChange(void* st, float newRate) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setRateChange(newRate);
}

void soundtouch_setTempoChange(void* st, float newTempo) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setTempoChange(newTempo);
}

void soundtouch_setPitch(void* st, float newPitch) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setPitch(newPitch);
}

void soundtouch_setPitchSemiTones(void* st, float newPitch) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setPitchSemiTones(newPitch);
}

void soundtouch_setPitchOctaves(void* st, float newPitch) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setPitchOctaves(newPitch);
}

void soundtouch_setChannels(void* st, int numChannels) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setChannels(numChannels);
}

void soundtouch_setSampleRate(void* st, int srate) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->setSampleRate(srate);
}

void soundtouch_flush(void* st) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->flush();
}

void soundtouch_putSamples(void* st, const float* samples, int numSamples) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->putSamples(samples, numSamples);
}

void soundtouch_clear(void* st) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    instance->clear();
}

int soundtouch_setSetting(void* st, int settingId, int value) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    return instance->setSetting(settingId, value);
}

int soundtouch_getSetting(void* st, int settingId) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    return instance->getSetting(settingId);
}

int soundtouch_numUnprocessedSamples(void* st) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    return instance->numUnprocessedSamples();
}

int soundtouch_receiveSamples(void* st, float* outBuffer, int maxSamples) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    return instance->receiveSamples(outBuffer, maxSamples);
}

int soundtouch_numSamples(void* st) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    return instance->numSamples();
}

int soundtouch_isEmpty(void* st) {
    SoundTouch* instance = static_cast<SoundTouch*>(st);
    return instance->isEmpty();
}

void* bpm_createInstance(int numChannels, int sampleRate) {
    return new BPMDetect(numChannels, sampleRate);
}

void bpm_destroyInstance(void* detector) {
    delete static_cast<BPMDetect*>(detector);
}

void bpm_putSamples(void* detector, const float *samples, int numSamples) {
    BPMDetect* bpm = static_cast<BPMDetect*>(detector);
    bpm->inputSamples(samples, numSamples);
}

float bpm_getBpm(void* detector) {
    BPMDetect* bpm = static_cast<BPMDetect*>(detector);
    return bpm->getBpm();
}
