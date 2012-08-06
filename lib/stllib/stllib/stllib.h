// stllib.h
#include "f2c.h"

#pragma once

/* f2c.h  --  Standard Fortran to C header file */

/**  barf  [ba:rf]  2.  "He suggested using FORTRAN, and everybody barfed."

	- From The Shogakukan DICTIONARY OF NEW ENGLISH (Second edition) */


using namespace System;


namespace stllib {
	public  ref class STL
	{
	public:
	 int   stl_(double *y, int *n, int *np, int *ns,
	 int *nt, int *nl, int *isdeg, int *itdeg, int *
	ildeg, int *nsjump, int *ntjump, int *nljump, int *ni,
	 int *no, double *rw, double *season, double *trend, 
	double *work);
	 void compute(double *x, int n, int period, int s_window, double* trend,double *season);
	private:
	 int stlest_(double *y, int *n, int *len, int 
	*ideg, double *xs, double *ys, int *nleft, int *
	nright, double *w, logical *userw, double *rw, logical *ok);
	
	 int stless_(double *y, int *n, int *len, int 
	*ideg, int *njump, logical *userw, double *rw, double *ys,
	 double *res);

	 int  stlfts_(double *x, int *n, int *np, 
	double *trend, double *work);

	 int stlma_(double *x, int *n, int *len, 
	double *ave);
	
	 int stlstp_(double *y, int *n, int *np, int *
	ns, int *nt, int *nl, int *isdeg, int *itdeg, int 
	*ildeg, int *nsjump, int *ntjump, int *nljump, int *
	ni, logical *userw, double *rw, double *season, double *
	trend, double *work);

	 int stlrwt_(double *y, int *n, double *fit, 
	double *rw);

	 int stlss_(double *y, int *n, int *np, int *
	ns, int *isdeg, int *nsjump, logical *userw, double *rw, 
	double *season, double *work1, double *work2, double *
	work3, double *work4);

	 int stlez_(double *y, int *n, int *np, int *
	ns, int *isdeg, int *itdeg, logical *robust, int *no, 
	double *rw, double *season, double *trend, double *
	work);

	 int psort_(double *a, int *n, int *ind, int *	ni);
	 
	};
}
