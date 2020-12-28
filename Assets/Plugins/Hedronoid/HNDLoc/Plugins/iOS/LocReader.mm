//
//  LocReader.m
//  locreader
//
//  Created by NapNok on 06/10/2017.
//  Copyright © 2017 NapNok. All rights reserved.
//

#import <Foundation/Foundation.h>

extern "C" {
    char* GetPreferredLanguages(){
            NSArray* langs = [NSLocale preferredLanguages];
            NSString* concat = [langs componentsJoinedByString:@";"];
            char* output = (char*)malloc(strlen([concat UTF8String])+1);
            strcpy(output, [concat UTF8String]);
            return output;
    }
}
