//
//  SoundTouchDylib.h
//  soundtouch-macos
//
//  Created by Tu Le on 4/11/24.
//

#ifndef SoundTouch_h
#define SoundTouch_h


#endif /* SoundTouch_h */

#ifdef __cplusplus
extern "C"
{
#endif

int soundtouch_getVersionId();
void* soundtouch_createInstance();
void soundtouch_destroyInstance(void* st);
const char* soundtouch_getVersionString();
void soundtouch_setRate(void* st, float newRate);
void soundtouch_setTempo(void* st, float newTempo);
void soundtouch_setRateChange(void* st, float newRate);
void soundtouch_setTempoChange(void* st, float newTempo);
void soundtouch_setPitch(void* st, float newPitch);
void soundtouch_setPitchSemiTones(void* st, float newPitch);
void soundtouch_setPitchOctaves(void* st, float newPitch);
void soundtouch_setChannels(void* st, int numChannels);
void soundtouch_setSampleRate(void* st, int srate);
void soundtouch_flush(void* st);
void soundtouch_putSamples(void* st, const float* samples, int numSamples);
void soundtouch_clear(void* st);
int soundtouch_setSetting(void* st, int settingId, int value);
int soundtouch_getSetting(void* st, int settingId);
int soundtouch_numUnprocessedSamples(void* st);
int soundtouch_receiveSamples(void* st, float* samples, int maxSamples);
int soundtouch_numSamples(void* st);
int soundtouch_isEmpty(void* st);

void* bpm_createInstance(int numChannels, int sampleRate);
void bpm_destroyInstance(void* detector);
void bpm_putSamples(void* detector, const float *samples, int numSamples);
float bpm_getBpm(void* detector);

#ifdef __cplusplus
}
#endif

#import <Foundation/Foundation.h>
